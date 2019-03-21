/*
OFX Cross Fade Compositor example plugin, a plugin that illustrates the use of the OFX Support library.

Portions Copyright (C) 2010-2011 Sony Creative Software Inc.

Portions Copyright (C) 2004-2005 The Open Effects Association Ltd
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

#include <stdio.h>
#include "ofxsImageEffect.h"
#include "ofxsMultiThread.h"

#include "../include/ofxsProcessing.H"

enum
{
    COMPOSITOR_FUNCTION_ADD = 0,
    COMPOSITOR_FUNCTION_SUB,
};

////////////////////////////////////////////////////////////////////////////////
// base class for the noise

/** @brief  Base class used to blend two images together */
class CompositorProcessorBase : public OFX::ImageProcessor {
protected :

    OFX::Image *_srcAImg; 
    OFX::Image *_srcBImg; 

    double _srcAMult;
    double _srcBMult;
    double _destMult;

    int _function;

public :
  /** @brief no arg ctor */
  CompositorProcessorBase(OFX::ImageEffect &instance)
    : OFX::ImageProcessor(instance)
    , _srcAImg(0)
    , _srcBImg(0)
    , _srcAMult(0.0)
    , _srcBMult(0.0)
    , _destMult(0.0)
    , _function(0)
  {        
  }

  /** @brief set the src image */
  void setSrcAImg(OFX::Image *v) {_srcAImg = v;}
  void setSrcBImg(OFX::Image *v) {_srcBImg = v;}

  /** @brief set the source multiplier */
  void setSrcAMult(double x) {_srcAMult = x;}

  /** @brief set the source multiplier */
  void setSrcBMult(double x) {_srcBMult = x;}

  /** @brief set the destination multiplier */
  void setDestMult(double x) {_destMult = x;}

  /** @brief set the destination multiplier */
  void setFunction(int x) {_function = x;}

};

template <class T> inline T 
Clamp(T v, int min, int max)
{
  if(v < T(min)) return T(min);
  if(v > T(max)) return T(max);
  return v;
}


/** @brief templated class to blend between two images */
template <class PIX, int nComponents, int max>
class CompositorProcessor : public CompositorProcessorBase {
public :
  // ctor
  CompositorProcessor(OFX::ImageEffect &instance) 
    : CompositorProcessorBase(instance)
  {}

  // and do some processing
  void multiThreadProcessImages(OfxRectI procWindow)
  {
    // push pixels
    for(int y = procWindow.y1; y < procWindow.y2; y++) 
    {
      if(_effect.abort()) break;

      PIX *dstPix = (PIX *) _dstImg->getPixelAddress(procWindow.x1, y);

      for(int x = procWindow.x1; x < procWindow.x2; x++) 
      {
          PIX *srcAPix = (PIX *)  (_srcAImg ? _srcAImg->getPixelAddress(x, y) : 0);
          PIX *srcBPix = (PIX *)  (_srcBImg ? _srcBImg->getPixelAddress(x, y) : 0);

          switch(_function)
          {
          case COMPOSITOR_FUNCTION_ADD:
            dstPix[0] = (PIX) Clamp((srcAPix[0] * _srcAMult + srcBPix[0] * _srcBMult) * _destMult, 0, max);
            dstPix[1] = (PIX) Clamp((srcAPix[1] * _srcAMult + srcBPix[1] * _srcBMult) * _destMult, 0, max);
            dstPix[2] = (PIX) Clamp((srcAPix[2] * _srcAMult + srcBPix[2] * _srcBMult) * _destMult, 0, max);
            dstPix[3] = (PIX) Clamp((srcAPix[3] * _srcAMult + srcBPix[3] * _srcBMult) * _destMult, 0, max);
            break;
          case COMPOSITOR_FUNCTION_SUB:
            dstPix[0] = (PIX) Clamp((srcAPix[0] * _srcAMult - srcBPix[0] * _srcBMult) * _destMult, 0, max);
            dstPix[1] = (PIX) Clamp((srcAPix[1] * _srcAMult - srcBPix[1] * _srcBMult) * _destMult, 0, max);
            dstPix[2] = (PIX) Clamp((srcAPix[2] * _srcAMult - srcBPix[2] * _srcBMult) * _destMult, 0, max);
            dstPix[3] = (PIX) Clamp((srcAPix[3] * _srcAMult - srcBPix[3] * _srcBMult) * _destMult, 0, max);
            break;
          }
          dstPix += nComponents;
      }
    }
  }

};

////////////////////////////////////////////////////////////////////////////////
/** @brief The plugin that does our work */
class CompositorPlugin : public OFX::ImageEffect {
protected :
  // do not need to delete these, the ImageEffect is managing them for us
  OFX::Clip *dstClip_;
  OFX::Clip *srcAClip_;
  OFX::Clip *srcBClip_;

  OFX::DoubleParam  *srcAMult_;
  OFX::DoubleParam  *srcBMult_;
  OFX::DoubleParam  *destMult_;

  OFX::ChoiceParam  *function_;

public :
  /** @brief ctor */
  CompositorPlugin(OfxImageEffectHandle handle)
    : ImageEffect(handle)
    , dstClip_(0)
    , srcAClip_(0)
    , srcBClip_(0)
    , srcAMult_(0)
    , srcBMult_(0)
    , destMult_(0)
    , function_(0)
  {
    dstClip_    = fetchClip("Output");
    srcAClip_   = fetchClip("SourceA");
    srcBClip_   = fetchClip("SourceB");
    srcAMult_   = fetchDoubleParam("SourceAMult");
    srcBMult_   = fetchDoubleParam("SourceBMult");
    destMult_   = fetchDoubleParam("DestMult");
    function_   = fetchChoiceParam("Function");
  }

  /* Override the render */
  virtual void render(const OFX::RenderArguments &args);

  /* set up and run a processor */
  void
    setupAndProcess(CompositorProcessorBase &, const OFX::RenderArguments &args);
};


////////////////////////////////////////////////////////////////////////////////
/** @brief render for the filter */

////////////////////////////////////////////////////////////////////////////////
// basic plugin render function, just a skelington to instantiate templates from

// make sure components are sane
static void
checkComponents(const OFX::Image &src,
                OFX::BitDepthEnum dstBitDepth,
                OFX::PixelComponentEnum dstComponents)
{
  OFX::BitDepthEnum      srcBitDepth     = src.getPixelDepth();
  OFX::PixelComponentEnum srcComponents  = src.getPixelComponents();

  // see if they have the same depths and bytes and all
  if(srcBitDepth != dstBitDepth || srcComponents != dstComponents)
    throw int(1); // HACK!! need to throw an sensible exception here!        
}

/* set up and run a processor */
void
CompositorPlugin::setupAndProcess(CompositorProcessorBase &processor, const OFX::RenderArguments &args)
{
  // get a dst image
  std::auto_ptr<OFX::Image>  dst(dstClip_->fetchImage(args.time));
  OFX::BitDepthEnum          dstBitDepth    = dst->getPixelDepth();
  OFX::PixelComponentEnum    dstComponents  = dst->getPixelComponents();

  // fetch the two source images
  std::auto_ptr<OFX::Image> srcAImg(srcAClip_->fetchImage(args.time));
  std::auto_ptr<OFX::Image> srcBImg(srcBClip_->fetchImage(args.time));

  // make sure bit depths are sane
  if(srcAImg.get()) checkComponents(*srcAImg, dstBitDepth, dstComponents);
  if(srcBImg.get()) checkComponents(*srcBImg, dstBitDepth, dstComponents);

  // get the transition value
  double srcAMult = srcAMult_->getValueAtTime(args.time);
  double srcBMult = srcBMult_->getValueAtTime(args.time);
  double destMult = destMult_->getValueAtTime(args.time);

  int function = 0;
  function_->getValueAtTime(args.time, function);

  // set the images
  processor.setDstImg(dst.get());
  processor.setSrcAImg(srcAImg.get());
  processor.setSrcBImg(srcBImg.get());

  // set the render window
  processor.setRenderWindow(args.renderWindow);

  // set the scales
  processor.setSrcAMult(srcAMult);
  processor.setSrcBMult(srcBMult);
  processor.setDestMult(destMult);
  processor.setFunction(function);

  // Call the base class process member, this will call the derived templated process code
  processor.process();
}

// the overridden render function
void
CompositorPlugin::render(const OFX::RenderArguments &args)
{
  // instantiate the render code based on the pixel depth of the dst clip
  OFX::BitDepthEnum       dstBitDepth    = dstClip_->getPixelDepth();
  OFX::PixelComponentEnum dstComponents  = dstClip_->getPixelComponents();

  // do the rendering
  if(dstComponents == OFX::ePixelComponentRGBA) {
    switch(dstBitDepth) {
case OFX::eBitDepthUByte : {      
  CompositorProcessor<unsigned char, 4, 255> fred(*this);
  setupAndProcess(fred, args);
                           }
                           break;

case OFX::eBitDepthUShort : {
  CompositorProcessor<unsigned short, 4, 65535> fred(*this);
  setupAndProcess(fred, args);
                            }                          
                            break;

case OFX::eBitDepthFloat : {
  CompositorProcessor<float, 4, 1> fred(*this);
  setupAndProcess(fred, args);
                           }
                           break;
    }
  }
  else {
    switch(dstBitDepth) {
case OFX::eBitDepthUByte : {
  CompositorProcessor<unsigned char, 1, 255> fred(*this);
  setupAndProcess(fred, args);
                           }
                           break;

case OFX::eBitDepthUShort : {
  CompositorProcessor<unsigned short, 1, 65535> fred(*this);
  setupAndProcess(fred, args);
                            }                          
                            break;

case OFX::eBitDepthFloat : {
  CompositorProcessor<float, 1, 1> fred(*this);
  setupAndProcess(fred, args);
                           }                          
                           break;
    }
  } // switch
}

mDeclarePluginFactory(CompositorExamplePluginFactory, {}, {});
using namespace OFX;

void CompositorExamplePluginFactory::describe(OFX::ImageEffectDescriptor &desc) 
{
  // basic labels
  desc.setLabels("OFX Compositor", "OFX Compositor", "OFX Compositor");
  desc.setPluginGrouping("OFX");

  // Say we are a general context
  desc.addSupportedContext(eContextGeneral);

  // Add supported pixel depths
  desc.addSupportedBitDepth(eBitDepthUByte);
  desc.addSupportedBitDepth(eBitDepthUShort);
  desc.addSupportedBitDepth(eBitDepthFloat);

  // set a few flags
  desc.setSingleInstance(false);
  desc.setHostFrameThreading(false);
  desc.setSupportsMultiResolution(true);
  desc.setSupportsTiles(true);
  desc.setTemporalClipAccess(false);
  desc.setRenderTwiceAlways(false);
  desc.setSupportsMultipleClipPARs(false);

}

void CompositorExamplePluginFactory::describeInContext(OFX::ImageEffectDescriptor &desc, ContextEnum context)
{
    // for compositors Sony Vegas is looking for two clips
    //   with names starting with "Source" 
    //   it can be "SourceA" and "SourceB"  or
    //   "Source1" and "Source2" or whatever you want...

  // we are a compositor, so define the sourceA input clip
  ClipDescriptor *srcAClip = desc.defineClip("SourceA");
  srcAClip->addSupportedComponent(ePixelComponentRGBA);
  srcAClip->addSupportedComponent(ePixelComponentAlpha);
  srcAClip->setTemporalClipAccess(false);
  srcAClip->setSupportsTiles(true);

  // we are a compositor, so define the sourceB input clip
  ClipDescriptor *srcBClip = desc.defineClip("SourceB");
  srcBClip->addSupportedComponent(ePixelComponentRGBA);
  srcBClip->addSupportedComponent(ePixelComponentAlpha);
  srcBClip->setTemporalClipAccess(false);
  srcBClip->setSupportsTiles(true);

  // create the mandated output clip
  ClipDescriptor *dstClip = desc.defineClip("Output");
  dstClip->addSupportedComponent(ePixelComponentRGBA);
  dstClip->addSupportedComponent(ePixelComponentAlpha);
  dstClip->setSupportsTiles(true);

  ChoiceParamDescriptor *funcParam = desc.defineChoiceParam("Function");
  funcParam->setLabels("Function", "Function", "Function");
  funcParam->appendOption("Add");
  funcParam->appendOption("Subtract");
  funcParam->setDefault(0);

  DoubleParamDescriptor *srcAMultParam = desc.defineDoubleParam("SourceAMult");
  srcAMultParam->setLabels("Source 1 Multiplier", "Source 1 Multiplier", "Source 1 Multiplier");
  srcAMultParam->setDefault(1.0);
  srcAMultParam->setRange(-5, 5);
  srcAMultParam->setIncrement(0.1);
  srcAMultParam->setDisplayRange(0, 2);
  srcAMultParam->setAnimates(true); // can animate
  srcAMultParam->setDoubleType(eDoubleTypeScale);

  DoubleParamDescriptor *srcBMultParam = desc.defineDoubleParam("SourceBMult");
  srcBMultParam->setLabels("Source 2 Multiplier", "Source 2 Multiplier", "Source 2 Multiplier");
  srcBMultParam->setDefault(1.0);
  srcBMultParam->setRange(-5, 5);
  srcBMultParam->setIncrement(0.1);
  srcBMultParam->setDisplayRange(0, 2);
  srcBMultParam->setAnimates(true); // can animate
  srcBMultParam->setDoubleType(eDoubleTypeScale);

  DoubleParamDescriptor *destMultParam = desc.defineDoubleParam("DestMult");
  destMultParam->setLabels("Output Multiplier", "Output Multiplier", "Output Multiplier");
  destMultParam->setDefault(0.5);
  destMultParam->setRange(-5, 5);
  destMultParam->setIncrement(0.1);
  destMultParam->setDisplayRange(0, 2);
  destMultParam->setAnimates(true); // can animate
  destMultParam->setDoubleType(eDoubleTypeScale); 

}

ImageEffect* CompositorExamplePluginFactory::createInstance(OfxImageEffectHandle handle, ContextEnum context)
{
  return new CompositorPlugin(handle);
}

namespace OFX 
{
  namespace Plugin 
  {
    void getPluginIDs(OFX::PluginFactoryArray &ids)
    {
        static CompositorExamplePluginFactory p("com.sonycreativesoftware:compositor", 1, 0);
        ids.push_back(&p);
    }
  }
}
