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
#include "ofxsHWNDInteract.h"

#include <Windows.h>
#include <Tchar.h>

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
// a dumb interact that just draw's a square you can drag

class KitchenSinkInteract : public OFX::HWNDInteract {
protected :
  HWND _hwndMain;

public :
  KitchenSinkInteract(OfxInteractHandle handle, OFX::ImageEffect* effect) 
     : OFX::HWNDInteract(handle)
  {
  }

  // overridden functions from OFX::HWNDInteract to do things
  virtual bool createWindow(const OFX::CreateWindowArgs &args, OFX::PropertySet &outArgs);
  virtual bool moveWindow(const OFX::MoveWindowArgs &args);
  virtual bool disposeWindow(const OFX::HWNDInteractArgs &args);
  virtual bool showWindow(const OFX::HWNDInteractArgs &args);

  void setGridPosition(double x, double y);
};

////////////////////////////////////////////////////////////////////////////////
// base class for the kitchen sink

/** @brief  Base class used to blend two images together */
class KitchenSinkGeneratorBase : public OFX::ImageProcessor {
protected :

  int        _tilewidth;
  int        _tileheight;
  bool       _square;
  RGBAValue  _color1;
  RGBAValue  _color2;
  Int2DValue _gridOffset;

public :
  /** @brief no arg ctor */
  KitchenSinkGeneratorBase(OFX::ImageEffect &instance)
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
class KitchenSinkGenerator : public KitchenSinkGeneratorBase {
public :
  // ctor
  KitchenSinkGenerator(OFX::ImageEffect &instance) 
    : KitchenSinkGeneratorBase(instance)
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
class KitchenSinkGeneratorBGRA : public KitchenSinkGeneratorBase {
public :
  // ctor
  KitchenSinkGeneratorBGRA(OFX::ImageEffect &instance) 
    : KitchenSinkGeneratorBase(instance)
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
class KitchenSinkPlugin : public OFX::ImageEffect 
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
  OFX::CustomParam   *customData_;
  OFX::StringParam   *stringParam_;
  OFX::GroupParam    *kitchenSinkGroup_;
  OFX::BooleanParam  *disableGroupTest_;
  OFX::ChoiceParam   *choiceParam_;

public :
  /** @brief ctor */
  KitchenSinkPlugin(OfxImageEffectHandle handle)
    : ImageEffect(handle)
    , dstClip_(0)
    , width_(0)
    , height_(0)
    , square_(0)
    , color1_(0)
    , color2_(0)
    , gridPosition_(0)
    , customData_(0)
    , stringParam_(0)
    , kitchenSinkGroup_(0)
    , disableGroupTest_(0)
    , choiceParam_(0)
  {
    dstClip_  = fetchClip("Output");
    width_                = fetchDoubleParam("Width");
    height_               = fetchDoubleParam("Height");
    square_               = fetchBooleanParam("Square");
    color1_               = fetchRGBAParam("Color1");
    color2_               = fetchRGBAParam("Color2");
    gridPosition_         = fetchDouble2DParam("GridPosition");
    customData_           = fetchCustomParam("customdata");
    stringParam_          = fetchStringParam("stringparam");
    kitchenSinkGroup_     = fetchGroupParam("checkerboardgroup");
    disableGroupTest_     = fetchBooleanParam("disablegrouptest");
    choiceParam_          = fetchChoiceParam("choiceparam");
  }

  /* Override the render */
  virtual void render(const OFX::RenderArguments &args);

  /* override changedParam */
  virtual void changedParam(const OFX::InstanceChangedArgs &args, const std::string &paramName);

  /* Override the clip preferences, we need to say we are setting the frame varying flag */
  virtual void getClipPreferences(OFX::ClipPreferencesSetter &clipPreferences);

  /* set up and run a processor */
  void setupAndProcess(KitchenSinkGeneratorBase &, const OFX::RenderArguments &args);

  /** @brief The get RoD action.  We flag an infinite rod */
  bool getRegionOfDefinition(const OFX::RegionOfDefinitionArguments &args, OfxRectD &rod);

  /** @brief we define a custom about box */
  bool invokeAbout();

  /** @brief we define a custom help */
  bool invokeHelp();
};


////////////////////////////////////////////////////////////////////////////////
/** @brief render for the filter */

////////////////////////////////////////////////////////////////////////////////
// basic plugin render function, just a skelington to instantiate templates from


/* set up and run a processor */
void
KitchenSinkPlugin::setupAndProcess(KitchenSinkGeneratorBase &processor, const OFX::RenderArguments &args)
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
KitchenSinkPlugin::getClipPreferences(OFX::ClipPreferencesSetter &clipPreferences)
{
  clipPreferences.setOutputFrameVarying(true);
}

/** @brief The get RoD action.  We flag an infinite rod */
bool 
KitchenSinkPlugin::getRegionOfDefinition(const OFX::RegionOfDefinitionArguments &args, OfxRectD &rod)
{
  // we can generate noise anywhere on the image plan, so set our RoD to be infinite
  rod.x1 = rod.y1 = -std::numeric_limits<double>::infinity(); // kOfxFlagInfiniteMin;
  rod.x2 = rod.y2 = std::numeric_limits<double>::infinity(); // kOfxFlagInfiniteMax;
  return true;
}

// the overridden render function
void
KitchenSinkPlugin::render(const OFX::RenderArguments &args)
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
              KitchenSinkGeneratorBGRA<unsigned char, 4, 255> fred(*this);
              setupAndProcess(fred, args);
            }
            break;

        case OFX::eBitDepthUShort : 
          {
            KitchenSinkGeneratorBGRA<unsigned short, 4, 65535> fred(*this);
            setupAndProcess(fred, args);
          }                          
          break;

        case OFX::eBitDepthFloat : 
          {
            KitchenSinkGeneratorBGRA<float, 4, 1> fred(*this);
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
              KitchenSinkGenerator<unsigned char, 4, 255> fred(*this);
              setupAndProcess(fred, args);
            }
            break;

        case OFX::eBitDepthUShort : 
          {
            KitchenSinkGenerator<unsigned short, 4, 65535> fred(*this);
            setupAndProcess(fred, args);
          }                          
          break;

        case OFX::eBitDepthFloat : 
          {
            KitchenSinkGenerator<float, 4, 1> fred(*this);
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
        KitchenSinkGenerator<unsigned char, 1, 255> fred(*this);
        setupAndProcess(fred, args);
      }
      break;

    case OFX::eBitDepthUShort : 
      {
        KitchenSinkGenerator<unsigned short, 1, 65536> fred(*this);
        setupAndProcess(fred, args);
      }                          
      break;

    case OFX::eBitDepthFloat : 
      {
        KitchenSinkGenerator<float, 1, 1> fred(*this);
        setupAndProcess(fred, args);
      }                          
      break;
    }
  } 
}

// we have changed a param
void
KitchenSinkPlugin::changedParam(const OFX::InstanceChangedArgs &args, const std::string &paramName)
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
  else if(paramName == "buttonmsgparam")
  {
      sendMessage(OFX::Message::eMessageMessage, "Message", "Hi User!");
  }
  else if(paramName == "buttonquestionparam")
  {
      sendMessage(OFX::Message::eMessageQuestion, "Question", "Do you want to press all the buttons?");
  }
  else if(paramName == "buttonslowparam")
  {
      progressStart("I will count to ten...");
      for(int i = 0; i < 10; i++)
      {
          Sleep(1000);
          if(! progressUpdate((i + 1) * 0.1))
              break;
      }
      progressEnd();
  }
  else if(paramName == "buttonforwardparam")
  {
      double t1, t2;
      timeLineGetBounds(t1, t2);
      double t = timeLineGetTime();
      timeLineGotoTime(t + 1.0);
  }
  else if(paramName == "buttonbackwardparam")
  {
      double t1, t2;
      timeLineGetBounds(t1, t2);
      double t = timeLineGetTime();
      timeLineGotoTime(t - 1.0);
  }
  else if(paramName == "buttonsetcustomparam")
  {
      std::string prev;
      customData_->getValue(prev);
      int count = 0;
      if(prev.substr(0, 12) == "custom value")
      {
          count = atoi(prev.substr(13).c_str());
      }
      char next[256];
      sprintf_s(next, 256, "custom value %d", count + 1);
      customData_->setValue(next);

      char msg[256];
      sprintf_s(msg, 256, "Custom was '%s' now it's '%s'", prev.c_str(), next);
      sendMessage(OFX::Message::eMessageMessage, "SetCustomMsg", msg);
  }
  else if(paramName == "buttonstreaminfo")
  {
      char msg[512];
      OfxPointD size = getProjectSize();
      sprintf_s(msg, 512, "Stream Info\nSize %f x %f\nFrame Rate(fps): %f\nPixel Aspect: %f\nDuration(frames): %f", 
          size.x, size.y, 
          getFrameRate(), getProjectPixelAspectRatio(), getEffectDuration());
      sendMessage(OFX::Message::eMessageMessage, "StreamInfoMsg", msg);
  }
  else if(paramName == "buttonreversestring")
  {
      std::string prev;
      stringParam_->getValue(prev);
      char next[256];
      int len = prev.length();
      for(int ix = 0; ix < len; ix++)
      {
          next[ix] = prev[len - ix - 1];
      }
      next[len] = 0;
      stringParam_->setValue(next);
  }
  else if(paramName == "disablegrouptest")
  {
      kitchenSinkGroup_->setEnabled(disableGroupTest_->getValue());
  }
  else if(paramName == "buttonchoicesetting")
  {
      char msg[512];
      int ixChoice = 0;
      choiceParam_->getValue(ixChoice);
      std::string sChoice;
      choiceParam_->getOption(ixChoice, sChoice);
      choiceParam_->resetOptions();

      bool resetWorked = choiceParam_->getNOptions() == 0;
      choiceParam_->appendOption("Option 1");
      choiceParam_->appendOption("Option 2");
      choiceParam_->appendOption("Option 3");
      choiceParam_->appendOption("Option 4");
      choiceParam_->appendOption("Option 5");

      sprintf_s(msg, 512, "Choice setting %d : %s\nReset Test worked: %s", ixChoice, sChoice.c_str(), resetWorked ? "PASSED" : "FAILED");
      sendMessage(OFX::Message::eMessageMessage, "ChoiceSettingMsg", msg);
  }
}

bool
KitchenSinkPlugin::invokeAbout()
{
    MessageBox(NULL, "Kitchen sink has everything, \nincluding it's own about box.", "Kitchen Sink About", MB_OK);
    return true;
}

bool
KitchenSinkPlugin::invokeHelp()
{
    MessageBox(NULL, "Kitchen sink has everything, \nincluding it's own help.", "Kitchen Sink Help", MB_OK);
    return true;
}

#define ID_BUTTON1 100

static LRESULT CALLBACK KitchenSkinInteractWndProc
(
    HWND          hwnd,
    UINT          uMsg,
    WPARAM        wParam,
    LPARAM        lParam
)
{
    LRESULT  lRet = 0;

    // get the pointer to the plug-in site windows 
    KitchenSinkInteract* interact = (KitchenSinkInteract*)GetWindowLongPtr(hwnd, GWLP_USERDATA); 

    switch (uMsg)
    {
    case WM_CREATE:
        break;

    case WM_DESTROY:
        break;

    case WM_COMMAND:
        {
            UINT  uId     = LOWORD(wParam);
            switch(uId)
            {
            case ID_BUTTON1:
                interact->setGridPosition(0.5, 0.5);
                interact->triggerUpdate();
                break;
            }
        }
        break;
    }

    lRet = DefWindowProc(hwnd, uMsg, wParam, lParam);
    return lRet;
}

bool KitchenSinkInteract::createWindow(const OFX::CreateWindowArgs &args, OFX::PropertySet &outArgs)
{
    static const TCHAR szWndClass[] = _T("KitchenSinkInteract.WndClass");
    WNDCLASS wc;

    HINSTANCE hinst = (HINSTANCE)GetWindowLongPtr(args.hwndParent, GWLP_HINSTANCE); 

    if ( ! GetClassInfo (hinst, szWndClass, &wc))
    {
        ZeroMemory (&wc, sizeof(wc));
        wc.lpfnWndProc   = KitchenSkinInteractWndProc;
        wc.hInstance     = hinst;
        wc.hCursor       = LoadCursor (NULL, IDC_ARROW);
        wc.lpszClassName = szWndClass;
        wc.hbrBackground = (HBRUSH)(COLOR_BTNFACE+1);
        RegisterClass (&wc);
    }

    _hwndMain = CreateWindowEx (
                     WS_EX_WINDOWEDGE | WS_EX_CONTROLPARENT,
                     szWndClass, NULL,
                     WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS | WS_TABSTOP, //| WM_HSCROLL | WM_VSCROLL,
                     0, 0, 200, 200,
                     args.hwndParent,
                     NULL,
                     hinst,
                     NULL);
    SetWindowLongPtr(_hwndMain, GWLP_USERDATA, (LONG_PTR)this); 

    HWND hctlBtn1;
    hctlBtn1 = CreateWindow(
        _T("Button"),
        _T("Kitchen Sink Button"),
        WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON, 
        10, 10, 180, 30,
        _hwndMain,
        (HMENU)UintToPtr(ID_BUTTON1),
        hinst,
        NULL);

    outArgs.propSetInt(kOfxHWndInteractPropMinSize, 200, 0);
    outArgs.propSetInt(kOfxHWndInteractPropMinSize, 200, 1);

    outArgs.propSetInt(kOfxHWndInteractPropPreferredSize, 200, 0);
    outArgs.propSetInt(kOfxHWndInteractPropPreferredSize, 200, 1);

    return true;
}

bool KitchenSinkInteract::moveWindow(const OFX::MoveWindowArgs &args)
{
    MoveWindow(_hwndMain, args.location.x1, args.location.y1, args.location.x2, args.location.y2, TRUE);
    return true;
}

bool KitchenSinkInteract::disposeWindow(const OFX::HWNDInteractArgs &args)
{
    DestroyWindow(_hwndMain);
    return true;
}

bool KitchenSinkInteract::showWindow(const OFX::HWNDInteractArgs &args)
{
    ShowWindow(_hwndMain, SW_SHOW);
    return true;
}

void KitchenSinkInteract::setGridPosition(double x, double y)
{
    OFX::Double2DParam* gridPosition = (OFX::Double2DParam*) _effect->getParam("GridPosition");
    if(gridPosition != NULL)
        gridPosition->setValue(x, y);
}



mDeclarePluginFactory(KitchenSinkExamplePluginFactory, {}, {});

using namespace OFX;

class KitchenSinkInteractDescriptor : public DefaultEffectHWNDInteractDescriptor<KitchenSinkInteractDescriptor, KitchenSinkInteract> {};

void KitchenSinkExamplePluginFactory::describe(OFX::ImageEffectDescriptor &desc) 
{
  desc.setLabels("OFX Kitchen Sink", "OFX Kitchen Sink", "OFX Kitchen Sink");
  desc.setPluginGrouping("Sony OFX");
  desc.addSupportedContext(eContextGenerator);
  desc.addSupportedContext(eContextFilter);
  desc.addSupportedContext(eContextGeneral);
  desc.addSupportedContext(eContextTransition);
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

  desc.setHWNDInteractDescriptor( new KitchenSinkInteractDescriptor);
}

void KitchenSinkExamplePluginFactory::describeInContext(OFX::ImageEffectDescriptor &desc, ContextEnum context) 
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

    // Define the mandated "Transition" param, note that we don't do anything with this other than.
    // describe it. It is not a true param but how the host indicates to the plug-in how far through
    // the transition it is. It appears on no plug-in side UI, it is purely the hosts to manage.
    DoubleParamDescriptor *transitionParam = desc.defineDoubleParam("Transition");
  }

  ClipDescriptor *dstClip = desc.defineClip("Output");
  dstClip->addSupportedComponent(ePixelComponentRGBA);
  dstClip->addSupportedComponent(ePixelComponentAlpha);
  dstClip->setSupportsTiles(true);
  dstClip->setFieldExtraction(eFieldExtractSingle);

  GroupParamDescriptor *checkerboardGroup = desc.defineGroupParam("checkerboardgroup");
  checkerboardGroup->setLabels("Checkerboard Controls", "Checkerboard Controls", "Checkerboard Controls");

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
  widthParam->setParent(*checkerboardGroup);

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
  heightParam->setParent(*checkerboardGroup);

  BooleanParamDescriptor *squareParam = desc.defineBooleanParam("Square");
  squareParam->setLabels("Square", "Square", "Square");
  squareParam->setScriptName("square");
  squareParam->setHint("Keep the checkerboard proportional.");
  squareParam->setDefault(true);
  squareParam->setAnimates(true); // can animate
  squareParam->setParent(*checkerboardGroup);

  RGBAParamDescriptor* color1Param = desc.defineRGBAParam("Color1");
  color1Param->setLabels("Color 1", "Color 1", "Color 1");
  color1Param->setScriptName("color1");
  color1Param->setHint("KitchenSink color 1.");
  color1Param->setDefault(0.0, 0.0, 0.0, 1.0);
  color1Param->setAnimates(true); // can animate
  color1Param->setParent(*checkerboardGroup);

  RGBAParamDescriptor* color2Param = desc.defineRGBAParam("Color2");
  color2Param->setLabels("Color 2", "Color 2", "Color 2");
  color2Param->setScriptName("color2");
  color2Param->setHint("KitchenSink color 2.");
  color2Param->setDefault(1.0, 1.0, 1.0, 1.0);
  color2Param->setAnimates(true); // can animate
  color2Param->setParent(*checkerboardGroup);

  Double2DParamDescriptor* gridPositionParam = desc.defineDouble2DParam("GridPosition");
  gridPositionParam->setLabels("Grid Position", "Grid Position", "Grid Position");
  gridPositionParam->setScriptName("gridPosition");
  gridPositionParam->setHint("Offset of the grid.");
  gridPositionParam->setDefault(0.0, 0.0);
  gridPositionParam->setRange(0.0, 0.0, 1.0, 1.0);
  gridPositionParam->setAnimates(true); // can animate
  gridPositionParam->setParent(*checkerboardGroup);


  GroupParamDescriptor *doubleParamGroup = desc.defineGroupParam("doubleparamgroup");
  doubleParamGroup->setLabels("Double Controls", "Double Controls", "Double Controls");

  DoubleParamDescriptor *sliderParam = desc.defineDoubleParam("slider");
  sliderParam->setLabels("1D Slider", "1D Slider", "1D Slider");
  sliderParam->setScriptName("doubleslider");
  sliderParam->setHint("Just some double parameter.");
  sliderParam->setDefault(0.0);
  sliderParam->setRange(-5.0, 5.0);
  sliderParam->setIncrement(0.1);
  sliderParam->setDisplayRange(-1.0, 1.0);
  sliderParam->setAnimates(true); // can animate
  sliderParam->setDoubleType(eDoubleTypeScale);
  sliderParam->setParent(*doubleParamGroup);

  Double2DParamDescriptor* slider2DParam = desc.defineDouble2DParam("slider2d");
  slider2DParam->setLabels("2D Slider", "2D Slider", "2D Slider");
  slider2DParam->setScriptName("slider2d");
  slider2DParam->setHint("Some 2D double parameter.");
  slider2DParam->setDefault(0.0, 0.0);
  slider2DParam->setRange(-5.0, -5.0, 5.0, 5.0);
  slider2DParam->setDisplayRange(-1.0, -1.0, 1.0, 1.0);
  slider2DParam->setAnimates(true); // can animate
  slider2DParam->setParent(*doubleParamGroup);

  Double3DParamDescriptor* slider3DParam = desc.defineDouble3DParam("slider3d");
  slider3DParam->setLabels("3D Slider", "3D Slider", "3D Slider");
  slider3DParam->setScriptName("slider3d");
  slider3DParam->setHint("Some 3D double parameter.");
  slider3DParam->setDefault(0.0, 0.0, 0.0);
  slider3DParam->setRange(-5.0, -5.0, -5.0, 5.0, 5.0, 5.0);
  slider3DParam->setDisplayRange(-1.0, -1.0, -1.0, 1.0, 1.0, 1.0);
  slider3DParam->setAnimates(true); // can animate
  slider3DParam->setParent(*doubleParamGroup);


  GroupParamDescriptor *integerParamGroup = desc.defineGroupParam("intparamgroup");
  integerParamGroup->setLabels("Integer Controls", "Integer Controls", "Integer Controls");

  IntParamDescriptor *intsliderParam = desc.defineIntParam("intslider");
  intsliderParam->setLabels("Integer 1D Slider", "Integer 1D Slider", "Integer 1D Slider");
  intsliderParam->setScriptName("doubleintslider");
  intsliderParam->setHint("Just some double parameter.");
  intsliderParam->setDefault(0);
  intsliderParam->setRange(0, 20);
  intsliderParam->setDisplayRange(0, 10);
  intsliderParam->setAnimates(true); // can animate
  intsliderParam->setParent(*integerParamGroup);

  Int2DParamDescriptor* intslider2DParam = desc.defineInt2DParam("intslider2d");
  intslider2DParam->setLabels("Integer 2D Slider", "Integer 2D Slider", "Integer 2D Slider");
  intslider2DParam->setScriptName("intslider2d");
  intslider2DParam->setHint("Some 2D integer parameter.");
  intslider2DParam->setDefault(0, 0);
  intslider2DParam->setRange(-100, -100, 100, 100);
  intslider2DParam->setDisplayRange(-10, -10, 10, 10);
  intslider2DParam->setAnimates(true); // can animate
  intslider2DParam->setParent(*integerParamGroup);

  Int3DParamDescriptor* intslider3DParam = desc.defineInt3DParam("intslider3d");
  intslider3DParam->setLabels("Integer 3D Slider", "Integer 3D Slider", "Integer 3D Slider");
  intslider3DParam->setScriptName("intslider3d");
  intslider3DParam->setHint("Some 3D integer parameter.");
  intslider3DParam->setDefault(5, 5, 5);
  intslider3DParam->setRange(-10, -10, -10, 10, 10, 10);
  intslider3DParam->setDisplayRange(0, 0, 0, 10, 10, 10);
  intslider3DParam->setAnimates(true); // can animate
  intslider3DParam->setParent(*integerParamGroup);

  GroupParamDescriptor *colorParamGroup = desc.defineGroupParam("colorparamgroup");
  colorParamGroup->setLabels("Color Controls", "Color Controls", "Color Controls");

  RGBAParamDescriptor* colorWithAlphaParam = desc.defineRGBAParam("colorwithalpha");
  colorWithAlphaParam->setLabels("Color With Alpha", "Color With Alpha", "Color With Alpha");
  colorWithAlphaParam->setScriptName("coloraithalpha");
  colorWithAlphaParam->setHint("Some color with alpha.");
  colorWithAlphaParam->setDefault(1.0, 0.0, 0.0, 1.0);
  colorWithAlphaParam->setAnimates(true); // can animate
  colorWithAlphaParam->setParent(*colorParamGroup);

  RGBParamDescriptor* colorWithoutAlphaParam = desc.defineRGBParam("colorwithoutalpha");
  colorWithoutAlphaParam->setLabels("Color Without Alpha", "Color Without Alpha", "Color Without Alpha");
  colorWithoutAlphaParam->setScriptName("colorwithoutalpha");
  colorWithoutAlphaParam->setHint("Color parameter without alpha.");
  colorWithoutAlphaParam->setDefault(0.0, 0.0, 1.0);
  colorWithoutAlphaParam->setAnimates(true); // can animate
  colorWithoutAlphaParam->setParent(*colorParamGroup);

  GroupParamDescriptor *otherParamGroup = desc.defineGroupParam("otherparamgroup");
  otherParamGroup->setLabels("Other Controls", "Other Controls", "Other Controls");

  ChoiceParamDescriptor *choiceParam = desc.defineChoiceParam("choiceparam");
  choiceParam->setLabels("Options", "Options", "Options");
  choiceParam->setHint("Some choice parameter.");
  choiceParam->appendOption("Option 1");
  choiceParam->appendOption("Option 2");
  choiceParam->appendOption("Option 3");
  choiceParam->appendOption("Option 4");
  choiceParam->appendOption("Option 5");
  choiceParam->setDefault(3);
  choiceParam->setParent(*otherParamGroup);

  BooleanParamDescriptor *boolParam = desc.defineBooleanParam("bool");
  boolParam->setLabels("Boolean", "Boolean", "Boolean");
  boolParam->setScriptName("bool");
  boolParam->setHint("Some boolean parameter.");
  boolParam->setDefault(true);
  boolParam->setAnimates(true); // can animate
  boolParam->setParent(*otherParamGroup);

  StringParamDescriptor *strParam = desc.defineStringParam("stringparam");
  strParam->setLabels("String", "String", "String");
  strParam->setScriptName("stringparam");
  strParam->setHint("Some string parameter.");
  strParam->setAnimates(true); // can animate
  strParam->setDefault("Default String");
  strParam->setParent(*otherParamGroup);

  PushButtonParamDescriptor *buttonParam = desc.definePushButtonParam("buttonparam");
  buttonParam->setLabels("Press Me", "Press Me", "Press Me");
  buttonParam->setScriptName("buttonparam");
  buttonParam->setHint("Some button.");
  buttonParam->setParent(*otherParamGroup);


  GroupParamDescriptor *actionParamGroup = desc.defineGroupParam("actionsparamgroup");
  actionParamGroup->setLabels("Actions", "Actions", "Actions");

  PushButtonParamDescriptor *buttonMsgParam = desc.definePushButtonParam("buttonmsgparam");
  buttonMsgParam->setLabels("Message", "Message", "Message");
  buttonMsgParam->setScriptName("buttonmsgparam");
  buttonMsgParam->setHint("Get a message.");
  buttonMsgParam->setParent(*actionParamGroup);
  
  PushButtonParamDescriptor *buttonQuestionParam = desc.definePushButtonParam("buttonquestionparam");
  buttonQuestionParam->setLabels("Question", "Question", "Question");
  buttonQuestionParam->setScriptName("buttonquestionparam");
  buttonQuestionParam->setHint("Ask a question.");
  buttonQuestionParam->setParent(*actionParamGroup);
  
  PushButtonParamDescriptor *buttonSlowParam = desc.definePushButtonParam("buttonslowparam");
  buttonSlowParam->setLabels("Takes Time", "Takes Time", "Takes Time");
  buttonSlowParam->setScriptName("buttonslowparam");
  buttonSlowParam->setHint("Do something that takes time.");
  buttonSlowParam->setParent(*actionParamGroup);
  
  PushButtonParamDescriptor *buttonForwardParam = desc.definePushButtonParam("buttonforwardparam");
  buttonForwardParam->setLabels("Forward", "Forward", "Forward");
  buttonForwardParam->setScriptName("buttonforwardparam");
  buttonForwardParam->setHint("Move forward 1 sec.");
  buttonForwardParam->setParent(*actionParamGroup);
  
  PushButtonParamDescriptor *buttonBackwardParam = desc.definePushButtonParam("buttonbackwardparam");
  buttonBackwardParam->setLabels("Backward", "Backward", "Backward");
  buttonBackwardParam->setScriptName("buttonbackwardparam");
  buttonBackwardParam->setHint("Move backward 1 sec.");
  buttonBackwardParam->setParent(*actionParamGroup);

  PushButtonParamDescriptor *buttonSetCustomParam = desc.definePushButtonParam("buttonsetcustomparam");
  buttonSetCustomParam->setLabels("Set Custom Data", "Set Custom Data", "Set Custom Data");
  buttonSetCustomParam->setHint("Test setting custom data.");
  buttonSetCustomParam->setParent(*actionParamGroup);

  PushButtonParamDescriptor *buttonReverseStringParam = desc.definePushButtonParam("buttonreversestring");
  buttonReverseStringParam->setLabels("Reverse String", "Reverse String", "Reverse String");
  buttonReverseStringParam->setHint("Reverse the string value.");
  buttonReverseStringParam->setParent(*actionParamGroup);

  PushButtonParamDescriptor *buttonStreamInfoParam = desc.definePushButtonParam("buttonstreaminfo");
  buttonStreamInfoParam->setLabels("Stream Info", "Stream Info", "Stream Info");
  buttonStreamInfoParam->setHint("Get stream info.");
  buttonStreamInfoParam->setParent(*actionParamGroup);

  PushButtonParamDescriptor *buttonChoiceSettingParam = desc.definePushButtonParam("buttonchoicesetting");
  buttonChoiceSettingParam->setLabels("Choice Setting", "Choice Setting", "Choice Setting");
  buttonChoiceSettingParam->setHint("Report choice string.");
  buttonChoiceSettingParam->setParent(*actionParamGroup);

  BooleanParamDescriptor *disableGroupParam = desc.defineBooleanParam("disablegrouptest");
  disableGroupParam->setLabels("Enable Checkerboard Group", "Enable Checkerboard Group", "Enable Checkerboard Group");
  disableGroupParam->setHint("Test enabling/disabling of a group.");
  disableGroupParam->setDefault(true);
  disableGroupParam->setAnimates(true); // can animate
  disableGroupParam->setParent(*actionParamGroup);

  CustomParamDescriptor *customDataParam = desc.defineCustomParam("customdata");
  customDataParam->setDefault("default custom data");
  

  PageParamDescriptor *page = desc.definePageParam("Controls");
  page->addChild(*widthParam);
  page->addChild(*heightParam);
  page->addChild(*squareParam);
  page->addChild(*color1Param);
  page->addChild(*color2Param);
  page->addChild(*gridPositionParam);
}

ImageEffect* KitchenSinkExamplePluginFactory::createInstance(OfxImageEffectHandle handle, ContextEnum context)
{
  return new KitchenSinkPlugin(handle);
}

namespace OFX
{
  namespace Plugin
  {
    void getPluginIDs(OFX::PluginFactoryArray &ids)
    {
      static KitchenSinkExamplePluginFactory p("com.sonycreativesoftware:kitchenSinkPlugin", 1, 0);
      ids.push_back(&p);
    }
  };
};
