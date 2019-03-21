/*
OFX Genereator example plugin, a plugin that illustrates the use of the OFX Support library.

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
#include <limits>
#include "ofxsImageEffect.h"
#include "ofxsMultiThread.h"

#include "../include/ofxsProcessing.H"


typedef struct RGBAValue
{
    double r;
    double g;
    double b;
    double a;

    RGBAValue() { r = 0.0; g = 0.0; b = 0.0; a = 1.0; }
    RGBAValue(double vr, double vg, double vb, double va) { r = vr; g = vg; b = vb; a = va; }
} RGBAValue;

typedef struct Double2DValue
{
    union
    {
        double value[2];
        struct 
        {
            double x;
            double y;
        };
    };

    Double2DValue(double vx, double vy) { x = vx; y = vy; }
} Double2DValue;

typedef struct Int2DValue
{
    union
    {
        int value[2];
        struct 
        {
            int x;
            int y;
        };
    };

    Int2DValue(int vx, int vy) { x = vx; y = vy; }
} Int2DValue;

////////////////////////////////////////////////////////////////////////////////
// base class for the noise

/** @brief  Base class used to blend two images together */
class CheckerboardGeneratorBase : public OFX::ImageProcessor {
protected :

  int        _tilewidth;
  int        _tileheight;
  bool       _square;
  RGBAValue  _color1;
  RGBAValue  _color2;
  Int2DValue _gridOffset;

public :
  /** @brief no arg ctor */
  CheckerboardGeneratorBase(OFX::ImageEffect &instance)
    : OFX::ImageProcessor(instance)
    , _tilewidth(8)
    , _tileheight(8)
    , _square(true)
    , _color1(0.0, 0.0, 0.0, 1.0)
    , _color2(1.0, 1.0, 1.0, 1.0)
    , _gridOffset(0, 0)
  {        
  }

  /** @brief set the dimensions of the tile */
  void setTileDimensions(int w, int h) {_tilewidth = w; _tileheight = h;}

  /** @brief set the dimensions of the tile */
  void setTileSquare(bool square) {_square = square;}

  /** @brief set the dimensions of the tile */
  void setColor1(double r, double g, double b, double a) {_color1.r = r; _color1.g = g; _color1.b = b; _color1.a = a;}

  /** @brief set the dimensions of the tile */
  void setColor2(double r, double g, double b, double a) {_color2.r = r; _color2.g = g; _color2.b = b; _color2.a = a;}

  /** @brief set the dimensions of the tile */
  void setGridOffset(int x, int y) {_gridOffset.x = x; _gridOffset.y = y;}

};

/** @brief templated class to blend between two images */
template <class PIX, int nComponents, int max>
class CheckerboardGenerator : public CheckerboardGeneratorBase {
public :
  // ctor
  CheckerboardGenerator(OFX::ImageEffect &instance) 
    : CheckerboardGeneratorBase(instance)
  {}

  // and do some processing
  void multiThreadProcessImages(OfxRectI procWindow)
  {
    // push pixels
    for(int y = procWindow.y1; y < procWindow.y2; y++) 
    {
      if(_effect.abort()) break;

      PIX *dstPix = (PIX *) _dstImg->getPixelAddress(procWindow.x1, y);
      int gy = (y + _gridOffset.y) / _tileheight;

      for(int x = procWindow.x1; x < procWindow.x2; x++) 
      {
          int gx = (x + _gridOffset.x) / _tilewidth;
          int g = (gx + gy) % 2;
          if(g == 0)
          {
              if(max == 1) // implies floating point, so don't clamp
              {
                  dstPix[0] = (PIX) _color1.r;
                  dstPix[1] = (PIX) _color1.g;
                  dstPix[2] = (PIX) _color1.b;
                  dstPix[3] = (PIX) _color1.a;
              }
              else
              {
                  dstPix[0] = (PIX) (_color1.r * max);
                  dstPix[1] = (PIX) (_color1.g * max);
                  dstPix[2] = (PIX) (_color1.b * max);
                  dstPix[3] = (PIX) (_color1.a * max);
              }
          }
          else
          {
              if(max == 1) // implies floating point, so don't clamp
              {
                  dstPix[0] = (PIX) _color2.r;
                  dstPix[1] = (PIX) _color2.g;
                  dstPix[2] = (PIX) _color2.b;
                  dstPix[3] = (PIX) _color2.a;
              }
              else
              {
                  dstPix[0] = (PIX) (_color2.r * max);
                  dstPix[1] = (PIX) (_color2.g * max);
                  dstPix[2] = (PIX) (_color2.b * max);
                  dstPix[3] = (PIX) (_color2.a * max);
              }
          }
          dstPix += nComponents;
      }
    }
  }

};

/** @brief templated class to blend between two images */
template <class PIX, int nComponents, int max>
class CheckerboardGeneratorBGRA : public CheckerboardGeneratorBase {
public :
  // ctor
  CheckerboardGeneratorBGRA(OFX::ImageEffect &instance) 
    : CheckerboardGeneratorBase(instance)
  {}

  // and do some processing
  void multiThreadProcessImages(OfxRectI procWindow)
  {
    // push pixels
    for(int y = procWindow.y1; y < procWindow.y2; y++) 
    {
      if(_effect.abort()) break;

      PIX *dstPix = (PIX *) _dstImg->getPixelAddress(procWindow.x1, y);
      int gy = (y + _gridOffset.y) / _tileheight;

      for(int x = procWindow.x1; x < procWindow.x2; x++) 
      {
          int gx = (x + _gridOffset.x) / _tilewidth;
          int g = (gx + gy) % 2;
          if(g == 0)
          {
              if(max == 1) // implies floating point, so don't clamp
              {
                  dstPix[0] = (PIX) _color1.b;
                  dstPix[1] = (PIX) _color1.g;
                  dstPix[2] = (PIX) _color1.r;
                  dstPix[3] = (PIX) _color1.a;
              }
              else
              {
                  dstPix[0] = (PIX) (_color1.b * max);
                  dstPix[1] = (PIX) (_color1.g * max);
                  dstPix[2] = (PIX) (_color1.r * max);
                  dstPix[3] = (PIX) (_color1.a * max);
              }
          }
          else
          {
              if(max == 1) // implies floating point, so don't clamp
              {
                  dstPix[0] = (PIX) _color2.b;
                  dstPix[1] = (PIX) _color2.g;
                  dstPix[2] = (PIX) _color2.r;
                  dstPix[3] = (PIX) _color2.a;
              }
              else
              {
                  dstPix[0] = (PIX) (_color2.b * max);
                  dstPix[1] = (PIX) (_color2.g * max);
                  dstPix[2] = (PIX) (_color2.r * max);
                  dstPix[3] = (PIX) (_color2.a * max);
              }
          }
          dstPix += nComponents;
      }
    }
  }

};

////////////////////////////////////////////////////////////////////////////////
/** @brief The plugin that does our work */
class CheckerboardPlugin : public OFX::ImageEffect 
{
protected :
  // do not need to delete these, the ImageEffect is managing them for us
  OFX::Clip *dstClip_;

  OFX::DoubleParam   *width_;
  OFX::DoubleParam   *height_;
  OFX::BooleanParam  *square_;
  OFX::RGBAParam     *color1_;
  OFX::RGBAParam     *color2_;
  OFX::Double2DParam *gridPosition_;

public :
  /** @brief ctor */
  CheckerboardPlugin(OfxImageEffectHandle handle)
    : ImageEffect(handle)
    , dstClip_(0)
    , width_(0)
    , height_(0)
    , square_(0)
    , color1_(0)
    , color2_(0)
    , gridPosition_(0)
  {
    dstClip_  = fetchClip("Output");
    width_                = fetchDoubleParam("Width");
    height_               = fetchDoubleParam("Height");
    square_               = fetchBooleanParam("Square");
    color1_               = fetchRGBAParam("Color1");
    color2_               = fetchRGBAParam("Color2");
    gridPosition_         = fetchDouble2DParam("GridPosition");
  }

  /* Override the render */
  virtual void render(const OFX::RenderArguments &args);

  /* override changedParam */
  virtual void changedParam(const OFX::InstanceChangedArgs &args, const std::string &paramName);

  /* Override the clip preferences, we need to say we are setting the frame varying flag */
  virtual void getClipPreferences(OFX::ClipPreferencesSetter &clipPreferences);

  /* set up and run a processor */
  void setupAndProcess(CheckerboardGeneratorBase &, const OFX::RenderArguments &args);

  /** @brief The get RoD action.  We flag an infinite rod */
  bool getRegionOfDefinition(const OFX::RegionOfDefinitionArguments &args, OfxRectD &rod);

  /** @brief Vegas requires conversion of keyframe data */
  void upliftVegasKeyframes(const OFX::SonyVegasUpliftArguments &upliftInfo);
};


////////////////////////////////////////////////////////////////////////////////
/** @brief render for the filter */

////////////////////////////////////////////////////////////////////////////////
// basic plugin render function, just a skelington to instantiate templates from


/* set up and run a processor */
void
CheckerboardPlugin::setupAndProcess(CheckerboardGeneratorBase &processor, const OFX::RenderArguments &args)
{
  // get a dst image
  std::auto_ptr<OFX::Image>  dst(dstClip_->fetchImage(args.time));
  OFX::BitDepthEnum         dstBitDepth    = dst->getPixelDepth();
  OFX::PixelComponentEnum   dstComponents  = dst->getPixelComponents();
  OfxRectI                  dstBounds      = dst->getBounds();

  // set the images
  processor.setDstImg(dst.get());

  // set the render window
  processor.setRenderWindow(args.renderWindow);

  // set parameter values
  double width = width_->getValueAtTime(args.time);
  double height = height_->getValueAtTime(args.time);
  int pixelwidth = (int)(width * (dstBounds.x2 - dstBounds.x1));
  int pixelheight = (int)(height * (dstBounds.y2 - dstBounds.y1));
  if(pixelwidth == 0) pixelwidth = 1;
  if(pixelheight == 0) pixelheight = 1;
  bool square = square_->getValueAtTime(args.time);
  processor.setTileDimensions(pixelwidth, square ? pixelwidth : pixelheight);
  RGBAValue color;
  color1_->getValueAtTime(args.time, color.r, color.g, color.b, color.a);
  processor.setColor1(color.r, color.g, color.b, color.a);
  color2_->getValueAtTime(args.time, color.r, color.g, color.b, color.a);
  processor.setColor2(color.r, color.g, color.b, color.a);
  double ox = 0.0;
  double oy = 0.0;
  gridPosition_->getValueAtTime(args.time, ox, oy);
  processor.setGridOffset((int)(ox * (dstBounds.x2 - dstBounds.x1)), (int)(oy * (dstBounds.y2 - dstBounds.y1)));

  // Call the base class process member, this will call the derived templated process code
  processor.process();
}

/* Override the clip preferences, we need to say we are setting the frame varying flag */
void 
CheckerboardPlugin::getClipPreferences(OFX::ClipPreferencesSetter &clipPreferences)
{
  clipPreferences.setOutputFrameVarying(true);
}

/** @brief The get RoD action.  We flag an infinite rod */
bool 
CheckerboardPlugin::getRegionOfDefinition(const OFX::RegionOfDefinitionArguments &args, OfxRectD &rod)
{
  // we can generate noise anywhere on the image plan, so set our RoD to be infinite
  rod.x1 = rod.y1 = -std::numeric_limits<double>::infinity(); // kOfxFlagInfiniteMin;
  rod.x2 = rod.y2 = std::numeric_limits<double>::infinity(); // kOfxFlagInfiniteMax;
  return true;
}

// the overridden render function
void
CheckerboardPlugin::render(const OFX::RenderArguments &args)
{
  // instantiate the render code based on the pixel depth of the dst clip
  OFX::BitDepthEnum       dstBitDepth    = dstClip_->getPixelDepth();
  OFX::PixelComponentEnum dstComponents  = dstClip_->getPixelComponents();
  OFX::PixelOrderEnum     dstOrder       = dstClip_->getPixelOrder();

  // do the rendering
  if(dstComponents == OFX::ePixelComponentRGBA) 
  {
      if(dstOrder == OFX::ePixelOrderBGRA)
      {
        switch(dstBitDepth) 
        {
        case OFX::eBitDepthUByte : 
            {      
              CheckerboardGeneratorBGRA<unsigned char, 4, 255> fred(*this);
              setupAndProcess(fred, args);
            }
            break;

        case OFX::eBitDepthUShort : 
          {
            CheckerboardGeneratorBGRA<unsigned short, 4, 65535> fred(*this);
            setupAndProcess(fred, args);
          }                          
          break;

        case OFX::eBitDepthFloat : 
          {
            CheckerboardGeneratorBGRA<float, 4, 1> fred(*this);
            setupAndProcess(fred, args);
          }
          break;
        }
      }
      else
      {
        switch(dstBitDepth) 
        {
        case OFX::eBitDepthUByte : 
            {      
              CheckerboardGenerator<unsigned char, 4, 255> fred(*this);
              setupAndProcess(fred, args);
            }
            break;

        case OFX::eBitDepthUShort : 
          {
            CheckerboardGenerator<unsigned short, 4, 65535> fred(*this);
            setupAndProcess(fred, args);
          }                          
          break;

        case OFX::eBitDepthFloat : 
          {
            CheckerboardGenerator<float, 4, 1> fred(*this);
            setupAndProcess(fred, args);
          }
          break;
        }
      }
  }
  else 
  {
    switch(dstBitDepth) 
    {
    case OFX::eBitDepthUByte : 
      {
        CheckerboardGenerator<unsigned char, 1, 255> fred(*this);
        setupAndProcess(fred, args);
      }
      break;

    case OFX::eBitDepthUShort : 
      {
        CheckerboardGenerator<unsigned short, 1, 65536> fred(*this);
        setupAndProcess(fred, args);
      }                          
      break;

    case OFX::eBitDepthFloat : 
      {
        CheckerboardGenerator<float, 1, 1> fred(*this);
        setupAndProcess(fred, args);
      }                          
      break;
    }
  } 
}

// we have changed a param
void
CheckerboardPlugin::changedParam(const OFX::InstanceChangedArgs &args, const std::string &paramName)
{
  if(paramName == "Square")
  {
      bool sq = square_->getValue();
      if(sq)
      {
          height_->setEnabled(false);
      }
      else
      {
          height_->setEnabled(true);
      }
  }
}


typedef float           SFDIBNUM;
typedef SFDIBNUM       *PSFDIBNUM;
typedef const SFDIBNUM *PCSFDIBNUM;

typedef struct tSFDIBPIXEL
{
    union
    {
        SFDIBNUM    afl[4];
        struct
        {
            SFDIBNUM    b;    // Blue value
            SFDIBNUM    g;    // Green value
            SFDIBNUM    r;    // Red value
            SFDIBNUM    a;    // Alpha value
        };
    };

    inline void Init(SFDIBNUM _r, SFDIBNUM _g, SFDIBNUM _b, SFDIBNUM _a)
    {
        r = _r;
        g = _g;
        b = _b;
        a = _a;
    }

    void MulAlpha()
    {
        // pre-multiply the alpha channel into this pixel
        r *= a;
        g *= a;
        b *= a;
    }
    void UnMulAlpha()
    {
        // UN-pre-multiply the alpha channel into this pixel
        if (0.0 != a)
        {
            r /= a;
            g /= a;
            b /= a;
        }
    }
} SFDIBPIXEL;
//hungarian: pxl

typedef SFDIBPIXEL *PSFDIBPIXEL;
typedef const SFDIBPIXEL *PCSFDIBPIXEL;

typedef struct tCHECKR_PROPS
{
    SFDIBPIXEL  foreColor;
    SFDIBPIXEL  backColor;
    double      dTileWidth, dTileHeight;
    double      dBlendAmountX, dBlendAmountY;
    double      dTileOffsetX, dTileOffsetY;
    bool        bSymmetricTiles;
    bool        bProportionalBlend;
} CHECKR_PROPS;

/** @brief Vegas requires conversion of keyframe data */
void CheckerboardPlugin::upliftVegasKeyframes(const OFX::SonyVegasUpliftArguments &upliftInfo)
{
    void*  pvData0 = upliftInfo.getKeyframeData     (0);
    int    ccData0 = upliftInfo.getKeyframeDataSize (0);
    double dData0  = upliftInfo.getKeyframeTime     (0);

    if(ccData0 != sizeof(CHECKR_PROPS))
        return;

    CHECKR_PROPS* pProps0 = (CHECKR_PROPS*)pvData0;

    width_->setValue(pProps0->dTileWidth);
    height_->setValue(pProps0->dTileHeight);
    square_->setValue(pProps0->bSymmetricTiles);
    color1_->setValue(pProps0->foreColor.r, pProps0->foreColor.g, pProps0->foreColor.b, pProps0->foreColor.a);
    color2_->setValue(pProps0->backColor.r, pProps0->backColor.g, pProps0->backColor.b, pProps0->backColor.a);
    gridPosition_->setValue(pProps0->dTileOffsetX, pProps0->dTileOffsetY);

    bool fWidthAnimates = false;
    bool fHeightAnimates = false;
    bool fSquareAnimates = false;
    bool fColor1Animates = false;
    bool fColor2Animates = false;
    bool fPositionAnimates = false;

    for(int idx = 1; idx < upliftInfo.keyframeCount; idx++)
    {
        void*  pvDataN = upliftInfo.getKeyframeData     (idx);
        int    ccDataN = upliftInfo.getKeyframeDataSize (idx);
        double dDataN  = upliftInfo.getKeyframeTime     (idx);
        CHECKR_PROPS* pPropsN = (CHECKR_PROPS*)pvData0;

        if(pProps0->dTileWidth != pPropsN->dTileWidth)
        {
            if(! fWidthAnimates)
                width_->setValueAtTime(dData0, pProps0->dTileWidth);
            width_->setValueAtTime(dDataN, pPropsN->dTileWidth);
            fWidthAnimates = true;
        }
        if(pProps0->dTileHeight != pPropsN->dTileHeight)
        {
            if(! fHeightAnimates)
                height_->setValueAtTime(dData0, pProps0->dTileHeight);
            height_->setValueAtTime(dDataN, pPropsN->dTileHeight);
            fHeightAnimates = true;
        }
        if(pProps0->bSymmetricTiles != pPropsN->bSymmetricTiles)
        {
            if(! fSquareAnimates)
                square_->setValueAtTime(dData0, pProps0->bSymmetricTiles);
            square_->setValueAtTime(dDataN, pPropsN->bSymmetricTiles);
            fSquareAnimates = true;
        }
        if((pProps0->foreColor.r != pPropsN->foreColor.r) ||
            (pProps0->foreColor.g != pPropsN->foreColor.g) ||
            (pProps0->foreColor.b != pPropsN->foreColor.b) ||
            (pProps0->foreColor.a != pPropsN->foreColor.a))
        {
            if(! fColor1Animates)
                color1_->setValueAtTime(dData0, pProps0->foreColor.r, pProps0->foreColor.g, pProps0->foreColor.b, pProps0->foreColor.a);
            color1_->setValueAtTime(dDataN, pPropsN->foreColor.r, pPropsN->foreColor.g, pPropsN->foreColor.b, pPropsN->foreColor.a);
            fColor1Animates = true;
        }
        if((pProps0->backColor.r != pPropsN->backColor.r) ||
            (pProps0->backColor.g != pPropsN->backColor.g) ||
            (pProps0->backColor.b != pPropsN->backColor.b) ||
            (pProps0->backColor.a != pPropsN->backColor.a))
        {
            if(! fColor2Animates)
                color2_->setValueAtTime(dData0, pProps0->backColor.r, pProps0->backColor.g, pProps0->backColor.b, pProps0->backColor.a);
            color2_->setValueAtTime(dDataN, pPropsN->backColor.r, pPropsN->backColor.g, pPropsN->backColor.b, pPropsN->backColor.a);
            fColor2Animates = true;
        }
        if((pProps0->dTileOffsetX != pPropsN->dTileOffsetX) ||
            (pProps0->dTileOffsetY != pPropsN->dTileOffsetY))
        {
            if(! fPositionAnimates)
                gridPosition_->setValueAtTime(dData0, pProps0->dTileOffsetX, pProps0->dTileOffsetY);
            gridPosition_->setValueAtTime(dDataN, pPropsN->dTileOffsetX, pPropsN->dTileOffsetY);
            fPositionAnimates = true;
        }

        pProps0 = pPropsN;
        dData0 = dDataN;
    }
}


mDeclarePluginFactory(CheckerboardExamplePluginFactory, {}, {});

using namespace OFX;

void CheckerboardExamplePluginFactory::describe(OFX::ImageEffectDescriptor &desc) 
{
  desc.setLabels("OFX Checkerboard", "OFX Checkerboard", "OFX Checkerboard");
  desc.setPluginGrouping("Sony OFX");
  desc.addSupportedContext(eContextGenerator);
  desc.addSupportedBitDepth(eBitDepthUByte);
  desc.addSupportedBitDepth(eBitDepthUShort);
  desc.addSupportedBitDepth(eBitDepthFloat);
  desc.addSupportedBitDepth(eBitDepthUByteBGRA);
  desc.addSupportedBitDepth(eBitDepthUShortBGRA);
  desc.addSupportedBitDepth(eBitDepthFloatBGRA);
  desc.setSingleInstance(false);
  desc.setHostFrameThreading(false);
  desc.setSupportsMultiResolution(true);
  desc.setSupportsTiles(true);
  desc.setTemporalClipAccess(false);
  desc.setRenderTwiceAlways(false);
  desc.setSupportsMultipleClipPARs(false);
  desc.setRenderTwiceAlways(false);
}

void CheckerboardExamplePluginFactory::describeInContext(OFX::ImageEffectDescriptor &desc, ContextEnum context) 
{
  ClipDescriptor *dstClip = desc.defineClip("Output");
  dstClip->addSupportedComponent(ePixelComponentRGBA);
  dstClip->addSupportedComponent(ePixelComponentAlpha);
  dstClip->setSupportsTiles(true);
  dstClip->setFieldExtraction(eFieldExtractSingle);

  DoubleParamDescriptor *widthParam = desc.defineDoubleParam("Width");
  widthParam->setLabels("Width", "Width", "Width");
  widthParam->setScriptName("width");
  widthParam->setHint("Width of the tile.");
  widthParam->setDefault(0.1);
  widthParam->setRange(0, 1);
  widthParam->setIncrement(0.1);
  widthParam->setDisplayRange(0, 1);
  widthParam->setAnimates(true); // can animate
  widthParam->setDoubleType(eDoubleTypeScale);

  DoubleParamDescriptor *heightParam = desc.defineDoubleParam("Height");
  heightParam->setLabels("Height", "Height", "Height");
  heightParam->setScriptName("height");
  heightParam->setHint("Height of the tile.");
  heightParam->setDefault(0.1);
  heightParam->setRange(0, 1);
  heightParam->setIncrement(0.1);
  heightParam->setDisplayRange(0, 1);
  heightParam->setEnabled(false);
  heightParam->setAnimates(true); // can animate
  heightParam->setDoubleType(eDoubleTypeScale);

  BooleanParamDescriptor *squareParam = desc.defineBooleanParam("Square");
  squareParam->setLabels("Square", "Square", "Square");
  squareParam->setScriptName("square");
  squareParam->setHint("Keep the checkerboard proportional.");
  squareParam->setDefault(true);
  squareParam->setAnimates(true); // can animate

  RGBAParamDescriptor* color1Param = desc.defineRGBAParam("Color1");
  color1Param->setLabels("Color 1", "Color 1", "Color 1");
  color1Param->setScriptName("color1");
  color1Param->setHint("Checkerboard color 1.");
  color1Param->setDefault(0.0, 0.0, 0.0, 1.0);
  color1Param->setAnimates(true); // can animate

  RGBAParamDescriptor* color2Param = desc.defineRGBAParam("Color2");
  color2Param->setLabels("Color 2", "Color 2", "Color 2");
  color2Param->setScriptName("color2");
  color2Param->setHint("Checkerboard color 2.");
  color2Param->setDefault(1.0, 1.0, 1.0, 1.0);
  color2Param->setAnimates(true); // can animate

  Double2DParamDescriptor* gridPositionParam = desc.defineDouble2DParam("GridPosition");
  gridPositionParam->setLabels("Grid Position", "Grid Position", "Grid Position");
  gridPositionParam->setScriptName("gridPosition");
  gridPositionParam->setHint("Offset of the grid.");
  gridPositionParam->setDefault(0.0, 0.0);
  gridPositionParam->setRange(0.0, 0.0, 1.0, 1.0);
  gridPositionParam->setAnimates(true); // can animate

  PageParamDescriptor *page = desc.definePageParam("Controls");
  page->addChild(*widthParam);
  page->addChild(*heightParam);
  page->addChild(*squareParam);
  page->addChild(*color1Param);
  page->addChild(*color2Param);
  page->addChild(*gridPositionParam);

  desc.addVegasUpgradePath("{DB10DAB1-5247-4194-B666-E076AD51BE19}");
}

ImageEffect* CheckerboardExamplePluginFactory::createInstance(OfxImageEffectHandle handle, ContextEnum context)
{
  return new CheckerboardPlugin(handle);
}

namespace OFX
{
  namespace Plugin
  {
    void getPluginIDs(OFX::PluginFactoryArray &ids)
    {
      static CheckerboardExamplePluginFactory p("com.sonycreativesoftware:checkerboardPlugin", 1, 0);
      ids.push_back(&p);
    }
  };
};
