/*
OFX Timeslice Example plugin, a plugin that illustrates the use of the OFX Support library.

Portions Copyright (C) 2010-2011 Sony Creative Software Inc.

Portions Copyright (C) 2007 The Open Effects Association Ltd
Author Bruno Nicoletti bruno@thefoundry.co.uk

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.
* Neither the name The Open Effects Association Ltd, nor the names of its 
contributors may be used to endorse or promote products derived from this
software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The Open Effects Association Ltd
1 Wardour St
London W1D 6PA
England


*/

#ifdef _WINDOWS
#include <windows.h>
#endif

#ifdef __APPLE__
#include <AGL/gl.h>
#else
#include <GL/gl.h>
#endif

#include <stdio.h>
#include "ofxsImageEffect.h"
#include "ofxsMultiThread.h"

#include "../include/ofxsProcessing.H"


// Base class for the RGBA and the Alpha processor
class TimeSliceBase : public OFX::ImageProcessor {
protected :
  OFX::Image* _srcImg;

  OFX::Clip*  _srcClip;
  OFX::Clip*  _src2Clip;

  int    _outputrows;
  int    _outputcolumns;
  int    _frameoffset;
  int    _framespacing;
  int    _tilewidth;
  int    _tileheight;

  double _time;
  double _timeperframe;

public :
  /** @brief no arg ctor */
  TimeSliceBase(OFX::ImageEffect &instance)
    : OFX::ImageProcessor(instance)
    , _srcImg(0)
    , _srcClip(0)
    , _src2Clip(0)
    , _tilewidth(8)
    , _tileheight(8)
    , _outputrows(2)
    , _outputcolumns(2)
    , _frameoffset(0)
    , _framespacing(1)
    , _time(0.0)
    , _timeperframe(0.1)
  {        
  }

  /** @brief set the src clip */
  void setSrcClip(OFX::Clip *v) {_srcClip = v;}

  /** @brief set the src clip */
  void setSrc2Clip(OFX::Clip *v) {_src2Clip = v;}

  /** @brief set the parameters */
  void setParams(int frameoffset, int framespacing, int rows, int columns) 
  { _outputrows = rows; _outputcolumns = columns; _frameoffset = frameoffset; _framespacing = framespacing; }

  /** @brief set the dimensions of the tile */
  void setTileDimensions(int w, int h) {_tilewidth = w; _tileheight = h;}

  /** @brief set the parameters */
  void setTiming(double time, double timeperframe) 
  { _time = time; _timeperframe = timeperframe; }
};

// template to do the RGBA processing
template <class PIX, int nComponents, int max>
class TimeCombiner : public TimeSliceBase {
public :
  // ctor
  TimeCombiner(OFX::ImageEffect &instance) 
    : TimeSliceBase(instance)
  {}

  // and do some processing
  void multiThreadProcessImages(OfxRectI procWindow)
  {
      for(int c = 0; c < _outputcolumns; c++)
      {
          if(_effect.abort()) break;

          if(c * _tilewidth > procWindow.x2)
              continue;

          if(c * _tilewidth + _tilewidth < procWindow.x1)
              continue;

          OFX::Clip* srcClip = _src2Clip != NULL && ((c % 2) == 1) ? _src2Clip : _srcClip;
          int temporal_c = _src2Clip != NULL ? c / 2 : c;

          for(int r = 0; r < _outputrows; r++)
          {
              if(_effect.abort()) break;

              if(r * _tileheight > procWindow.y2)
                  continue;

              if(r * _tileheight + _tileheight < procWindow.y1)
                  continue;

              int frameoffset = (temporal_c + r * _outputcolumns) * _framespacing + _frameoffset;
              std::auto_ptr<OFX::Image> src(srcClip->fetchImage(_time + frameoffset));

              int x0 = c * _tilewidth;
              int y0 = r * _tileheight;

              int x1 = max(procWindow.x1, x0);
              int x2 = min(procWindow.x2, x0 + _tilewidth);
              int y1 = max(procWindow.y1, y0);
              int y2 = min(procWindow.y2, y0 + _tileheight);

              for(int y = y1; y < y2; y++) 
              {
                  if(_effect.abort()) break;
                  PIX *dstPix = (PIX *) _dstImg->getPixelAddress(x1, y);

                  for(int x = x1; x < x2; x++) 
                  {
                      PIX *srcPix = (PIX *)  (src.get() ? src->getPixelAddress((x - x0) * _outputcolumns, (y - y0) * _outputrows) : 0);

                      // do we have a source image to scale up
                      if(srcPix) 
                      {
                          for(int c = 0; c < nComponents; c++) 
                          {
                              dstPix[c] = srcPix[c];
                          }
                      }
                      else 
                      {
                        // no src pixel here, be black and transparent
                          for(int c = 0; c < nComponents; c++) 
                          {
                              dstPix[c] = 0;
                          }
                      }

                      // increment the dst pixel
                      dstPix += nComponents;
                  }
              }
          }
      }
  }
};

////////////////////////////////////////////////////////////////////////////////
/** @brief The plugin that does our work */
class TimeSlicePlugin : public OFX::ImageEffect {
protected :
  // do not need to delete these, the ImageEffect is managing them for us
  OFX::Clip *dstClip_;
  OFX::Clip *srcClip_;
  OFX::Clip *src2Clip_;

  OFX::IntParam*     outputrows_;
  OFX::IntParam*     outputcolumns_;
  OFX::IntParam*     frameoffset_;
  OFX::IntParam*     framespacing_;

public :
  /** @brief ctor */
  TimeSlicePlugin(OfxImageEffectHandle handle)
    : ImageEffect(handle)
    , dstClip_(0)
    , srcClip_(0)
    , src2Clip_(0)
    , outputrows_(0)
    , outputcolumns_(0)
    , frameoffset_(0)
    , framespacing_(0)
  {
    dstClip_ = fetchClip("Output");
    OFX::ContextEnum context = getContext();
    if(context == OFX::eContextFilter)
    {
      srcClip_ = fetchClip("Source");
    }
    else if(context == OFX::eContextGeneral)
    {
      srcClip_ = fetchClip("SourceA");
      src2Clip_ = fetchClip("SourceB");
    }
    else if(context == OFX::eContextTransition)
    {
      srcClip_ = fetchClip("SourceFrom");
      src2Clip_ = fetchClip("SourceTo");
    }

    outputrows_    = fetchIntParam("outputrows");
    outputcolumns_ = fetchIntParam("outputcolumns");
    frameoffset_   = fetchIntParam("frameoffset");
    framespacing_  = fetchIntParam("framespacing");
  }

  /* Override the render */
  virtual void render(const OFX::RenderArguments &args);

  /* set up and run a processor */
  void setupAndProcess(TimeSliceBase &, const OFX::RenderArguments &args);
};


////////////////////////////////////////////////////////////////////////////////
/** @brief render for the filter */

////////////////////////////////////////////////////////////////////////////////
// basic plugin render function, just a skelington to instantiate templates from


/* set up and run a processor */
void
TimeSlicePlugin::setupAndProcess(TimeSliceBase &processor, const OFX::RenderArguments &args)
{
  // get a dst image
  std::auto_ptr<OFX::Image> dst(dstClip_->fetchImage(args.time));
  OFX::BitDepthEnum       dstBitDepth    = dst->getPixelDepth();
  OFX::PixelComponentEnum dstComponents  = dst->getPixelComponents();
  OfxRectI                dstBounds      = dst->getBounds();

  // fetch main input image
  std::auto_ptr<OFX::Image> src(srcClip_->fetchImage(args.time));

  // make sure bit depths are sane
  if(src.get()) {
    OFX::BitDepthEnum       srcBitDepth   = src->getPixelDepth();
    OFX::PixelComponentEnum srcComponents = src->getPixelComponents();

    // see if they have the same depths and bytes and all
    if(srcBitDepth != dstBitDepth || srcComponents != dstComponents)
      throw int(1); // HACK!! need to throw an sensible exception here!
  }

  int outputrows    = outputrows_->getValueAtTime(args.time);
  int outputcolumns = outputcolumns_->getValueAtTime(args.time);
  int frameoffset   = frameoffset_->getValueAtTime(args.time);
  int framespacing  = framespacing_->getValueAtTime(args.time);
  processor.setParams(frameoffset, framespacing, outputrows, outputcolumns);

  int pixelwidth  = (dstBounds.x2 - dstBounds.x1) / outputcolumns;
  int pixelheight = (dstBounds.y2 - dstBounds.y1) / outputrows;
  processor.setTileDimensions(pixelwidth, pixelheight);

  // set the images, clips, parameters
  processor.setDstImg(dst.get());
  processor.setSrcClip(srcClip_);
  OFX::ContextEnum context = getContext();
  if(context == OFX::eContextGeneral || context == OFX::eContextTransition)
  {
      processor.setSrc2Clip(src2Clip_);
  }
  processor.setTiming(args.time, 1.0 / srcClip_->getFrameRate());

  // set the render window
  processor.setRenderWindow(args.renderWindow);

  // Call the base class process member, this will call the derived templated process code
  processor.process();
}

// the overridden render function
void
TimeSlicePlugin::render(const OFX::RenderArguments &args)
{
  // instantiate the render code based on the pixel depth of the dst clip
  OFX::BitDepthEnum       dstBitDepth    = dstClip_->getPixelDepth();
  OFX::PixelComponentEnum dstComponents  = dstClip_->getPixelComponents();

  // do the rendering
  if(dstComponents == OFX::ePixelComponentRGBA) {
    switch(dstBitDepth) {
case OFX::eBitDepthUByte : {      
  TimeCombiner<unsigned char, 4, 255> fred(*this);
  setupAndProcess(fred, args);
                           }
                           break;

case OFX::eBitDepthUShort : {
  TimeCombiner<unsigned short, 4, 65535> fred(*this);
  setupAndProcess(fred, args);
                            }                          
                            break;

case OFX::eBitDepthFloat : {
  TimeCombiner<float, 4, 1> fred(*this);
  setupAndProcess(fred, args);
                           }
                           break;
    }
  }
  else {
    switch(dstBitDepth) {
case OFX::eBitDepthUByte : {
  TimeCombiner<unsigned char, 1, 255> fred(*this);
  setupAndProcess(fred, args);
                           }
                           break;

case OFX::eBitDepthUShort : {
  TimeCombiner<unsigned short, 1, 65536> fred(*this);
  setupAndProcess(fred, args);
                            }                          
                            break;

case OFX::eBitDepthFloat : {
  TimeCombiner<float, 1, 1> fred(*this);
  setupAndProcess(fred, args);
                           }                          
                           break;
    }
  } 
}

mDeclarePluginFactory(TimeSliceExamplePluginFactory, {}, {});

using namespace OFX;
void TimeSliceExamplePluginFactory::describe(OFX::ImageEffectDescriptor &desc)
{
  // basic labels
  desc.setLabels("TimeSlice", "TimeSlice", "TimeSlice");
  desc.setPluginGrouping("OFX");

  // add the supported contexts, only filter at the moment
  desc.addSupportedContext(eContextFilter);
  desc.addSupportedContext(eContextGeneral);
  desc.addSupportedContext(eContextTransition);

  // add supported pixel depths
  desc.addSupportedBitDepth(eBitDepthUByte);
  desc.addSupportedBitDepth(eBitDepthUShort);
  desc.addSupportedBitDepth(eBitDepthFloat);

  // set a few flags
  desc.setSingleInstance(false);
  desc.setHostFrameThreading(false);
  desc.setSupportsMultiResolution(true);
  desc.setSupportsTiles(true);
  desc.setTemporalClipAccess(true);
  desc.setRenderTwiceAlways(false);
  desc.setSupportsMultipleClipPARs(false);

}

void TimeSliceExamplePluginFactory::describeInContext(OFX::ImageEffectDescriptor &desc, OFX::ContextEnum context)
{
  // Source clip only in the filter context
  // create the mandated source clip
  if(context == OFX::eContextFilter)
  {
    ClipDescriptor *srcClip = desc.defineClip("Source");
    srcClip->addSupportedComponent(ePixelComponentRGBA);
    srcClip->addSupportedComponent(ePixelComponentAlpha);
    srcClip->setTemporalClipAccess(false);
    srcClip->setSupportsTiles(true);
    srcClip->setIsMask(false);
  }
  else if(context == OFX::eContextGeneral)
  {
    ClipDescriptor *srcAClip = desc.defineClip("SourceA");
    srcAClip->addSupportedComponent(ePixelComponentRGBA);
    srcAClip->addSupportedComponent(ePixelComponentAlpha);
    srcAClip->setTemporalClipAccess(false);
    srcAClip->setSupportsTiles(true);
    srcAClip->setIsMask(false);

    ClipDescriptor *srcBClip = desc.defineClip("SourceB");
    srcBClip->addSupportedComponent(ePixelComponentRGBA);
    srcBClip->addSupportedComponent(ePixelComponentAlpha);
    srcBClip->setTemporalClipAccess(false);
    srcBClip->setSupportsTiles(true);
    srcBClip->setIsMask(false);
  }
  else if(context == OFX::eContextTransition)
  {
    ClipDescriptor *srcFromClip = desc.defineClip("SourceFrom");
    srcFromClip->addSupportedComponent(ePixelComponentRGBA);
    srcFromClip->addSupportedComponent(ePixelComponentAlpha);
    srcFromClip->setTemporalClipAccess(false);
    srcFromClip->setSupportsTiles(true);
    srcFromClip->setIsMask(false);

    ClipDescriptor *srcToClip = desc.defineClip("SourceTo");
    srcToClip->addSupportedComponent(ePixelComponentRGBA);
    srcToClip->addSupportedComponent(ePixelComponentAlpha);
    srcToClip->setTemporalClipAccess(false);
    srcToClip->setSupportsTiles(true);
    srcToClip->setIsMask(false);
  }

  // create the mandated output clip
  ClipDescriptor *dstClip = desc.defineClip("Output");
  dstClip->addSupportedComponent(ePixelComponentRGBA);
  dstClip->addSupportedComponent(ePixelComponentAlpha);
  dstClip->setSupportsTiles(true);

  IntParamDescriptor *framespacingParam = desc.defineIntParam("framespacing");
  framespacingParam->setLabels("Frame Spacing", "Frame Spacing", "Frame Spacing");
  framespacingParam->setScriptName("framespacing");
  framespacingParam->setHint("Frames Between each slice.");
  framespacingParam->setDefault(1);
  framespacingParam->setRange(-60, 60);
  framespacingParam->setDisplayRange(1, 10);
  framespacingParam->setAnimates(true); // can animate

  IntParamDescriptor *frameoffsetParam = desc.defineIntParam("frameoffset");
  frameoffsetParam->setLabels("Frame Offset", "Frame Offset", "Frame Offset");
  frameoffsetParam->setScriptName("frameoffset");
  frameoffsetParam->setHint("Frames Between each slice.");
  frameoffsetParam->setDefault(0);
  frameoffsetParam->setRange(-60, 60);
  frameoffsetParam->setDisplayRange(-10, 10);
  frameoffsetParam->setAnimates(true); // can animate

  IntParamDescriptor *outputcolumnsParam = desc.defineIntParam("outputcolumns");
  outputcolumnsParam->setLabels("Output Columns", "Output Columns", "Output Columns");
  outputcolumnsParam->setScriptName("outputcolumns");
  outputcolumnsParam->setHint("Columns in the output.");
  outputcolumnsParam->setDefault(4);
  outputcolumnsParam->setRange(1, 8);
  outputcolumnsParam->setDisplayRange(1, 8);
  outputcolumnsParam->setAnimates(true); // can animate

  IntParamDescriptor *outputrowsParam = desc.defineIntParam("outputrows");
  outputrowsParam->setLabels("Output Rows", "Output Rows", "Output Rows");
  outputrowsParam->setScriptName("outputrows");
  outputrowsParam->setHint("Rows in the output.");
  outputrowsParam->setDefault(4);
  outputrowsParam->setRange(1, 8);
  outputrowsParam->setDisplayRange(1, 8);
  outputrowsParam->setAnimates(true); // can animate


  if(context == OFX::eContextTransition)
  {
      // Define the mandated "Transition" param, note that we don't do anything with this other than.
      // describe it. It is not a true param but how the host indicates to the plug-in how far through
      // the transition it is. It appears on no plug-in side UI, it is purely the hosts to manage.
      DoubleParamDescriptor *param = desc.defineDoubleParam("Transition");
  }
}

OFX::ImageEffect* TimeSliceExamplePluginFactory::createInstance(OfxImageEffectHandle handle, OFX::ContextEnum context)
{
  return new TimeSlicePlugin(handle);
}

namespace OFX 
{
  namespace Plugin 
  {  
    void getPluginIDs(OFX::PluginFactoryArray &ids)
    {
      static TimeSliceExamplePluginFactory p("com.sonycreativesoftware.com:timeSlicePlugin", 1, 0);
      ids.push_back(&p);
    }
  }
}
