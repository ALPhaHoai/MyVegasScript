
Vegas Video Plug-In Software Development Kit (SDK)
January 2013


Documentation
-------------

Please read the "Sony Vegas Video Plug-in SDK.doc" file.

It's a good idea to review the OFX documentation on-line to understand basic priciples of the 
system. (http://openfx.sourceforge.net/Documentation/index.html)

If you have further questions, comments, or bug reports regarding the Vegas Video Plug-In SDK
you may contact us at: SCS-VideoPIDK@am.sony.com.



What's in this package
----------------------

SonyOfxPIDK\README.txt                       - this file
SonyOfxPIDK\Video Effects SDK EULA.pdf       - License Agreement for Sony Video Effects Software Development Kit
SonyOfxPIDK\SDK Excluded Software.txt        - list of files excluded from Sony Video Effects SDK License Agreement
SonyOfxPIDK\Sony Vegas Video Plug-in SDK.doc - documentation on making plug-ins for Vegas
SonyOfxPIDK\Include\*.h                      - these files are mostly from the standard OFX includes
SonyOfxPIDK\Library\*.*                      - this is a support library for building OFX Plug-ins 
                                               from The Foundry (http://www.thefoundry.co.uk)
                                               with extensions for Sony Vegas
SonyOfxPIDK\Plugins\Basic                    - example of a basic filter plug-in
SonyOfxPIDK\Plugins\Compositor               - example of a basic compositor plug-in
SonyOfxPIDK\Plugins\Field                    - example of a basic filter plug-in
SonyOfxPIDK\Plugins\Generator                - example of a basic generator plug-in
SonyOfxPIDK\Plugins\Generator-Checkerboard   - example of a basic generator plug-in
SonyOfxPIDK\Plugins\KitchenSink              - example of a multicontext plug-in with all the 
                                               different parameter types, custom UI, progress use, 
                                               message use
SonyOfxPIDK\Plugins\Invert                   - example of a basic filter plug-in
SonyOfxPIDK\Plugins\MultiBundle              - example of multiple plug-ins in a single package
SonyOfxPIDK\Plugins\TimeSlice                - example of a combination filter, transition, and
                                               compositor with temporal frame access
SonyOfxPIDK\Plugins\Transition               - example of a basic crossfade transition plug-in


About the Examples
------------------

Examples are provided with Microsoft Visual Studio 2008 projects to build in Debug and Release, Win32 and x64 
mode. The makefiles and *.plist files are provided if you wish to build your plug-in for Linux or 
Macintosh. 


Changes for Vegas Pro 12.0 Final
--------------------------------

Added handles for 2D and 3D points on the Video Preview window.

Added ability for plug-ins to use a display thumbnail that has alpha in it.

Added ability to set the default color space on RGB and RGBA parameters.

Added ability for optional mask clip inputs on filters.

Added support for the OpenGL rendering suite.



Changes for Vegas Pro 11.0 Final
--------------------------------

Improved display time for UI.

Added new parameter subtypes: a RichTextBox (data in RTF form) for string parameters, and
a Chrominance mode and Polar mode for double 2D parameters.

Ability to get the current Vegas render quality.

Ability to get the current Vegas context type for the plug-in (track, media, event, project, etc)

Ability to get/set Vegas interpolation types on keyframes as well as slopes on split and manual mode 
keyframes.

Add support for OFX message suite V2 with persistent messages.

Add support for OFX custom parameter interpolation.

Fixed RenderScale to indicate when we're doing half/quarter/eighth resolution.

Show plug-in grouping in the Vegas UI.



Changes for Vegas Pro 10.0d Final
---------------------------------

Fixed: Transition Progress Envelopes don't work for OFX Transitions.

Fixed: Preset thumbnails had incorrect OFX properties and sometimes black input frames.

Fixed: Crash if plug-in tried accessing properties in the destroyInstance action.

Fixed: Project Extents and Size were not setup for the IsIdentity call before a Render call.

Fixed: During generation of preset preview thumbnails, temporal frame requests would fail.

Fixed: Project Extents and Size were in pixel coordinated instead of canonical coordinates.

Fixed: Message suite would crash Vegas with a NULL effect handle, and would not work in Describe
and DescribeInContext calls.

Fixed: For Transitions and Compositors, effects that were not float compatible still got float
image data.

Fixed: Multithreading conflict with setting large string or custom data parameters.

Fixed: Multithreading conflict with effects that have Double field input setting that would 
corrupt the image data.

Fixed: Crash when opening a new project when the video effect window was open on a project 
output effect.

Fixed: Could not disable Transitions on a 3D project.

Fixed: Setting the IsHidden parameter on a group during a InstanceChanged call did nothing.

Fixed: Changing project frame rate would move keyframes in time.

Fixed: Clip property kOfxImageClipPropFieldOrder was always set to kOfxImageFieldNone, will now
return proper order in interlaced processing mode.

Fixed: Allow kOfxImagePropPixelOrder to be queried when images are not available.

Fixed: Plug-ins that were marked as non-threadsafe were accidentally being called from two different
treads at the same time.

Internal speed improvement for Vegas multi-threaded rendering to use a cache of instances instead
of creating a new instance for each frame it needed rendering.

Fixed: Default field extraction of a clip fixed to be Doubled.

Fixed: Frame numbers passed to render are now properly rounded to 0.0 or 0.5.

Fixed: Missing or unimplemented properties now return kOfxStatErrUnknown instead of kOfxStatErrValue.

Fixed: A crashing bug sometimes with interlaced processing.

Fixed: A crashing bug when clipGetImage was called from an InstanceChanged Action.

Fixed: Generators marked with output clip being FrameVarying would not update in the UI when parameter
values changed.

Fixed: Generators not updating their thumbnails when parameters changed.


Known Issues with Vegas Pro 10.0d Final
---------------------------------------

kOfxPropVersionLabel on Host is not supported. (Use kOfxPropAPIVersion to get version number).






Changes for Vegas Pro 10.0b
---------------------------

Fixed: Sync private data is called before cloning the effect for multithreaded rendering.

Fixed: paramDefine will return kOfxStatErrExists if the parameter is already defined.

Fixed: Presets now saved in a language invariant way (they were saved and loaded improperly
for locales that used something other than '.' as the factional seperator).

Fixed: GetClipPreferences is called to get the FrameVarying flag for effects that animate/change
even though their parameters have not changed value.

Fixed: FrameStep now returns 0.5 when processing in half frames.

Fixed: Custom values that were longer than 200 chars were not being saved properly in preset files.

Fixed: paramEditBegin and paramEditEnd now create undo blocks in the UI.

Fixed: IsSecret parameters if animated show up on the timeline.

Fixed: bug with plug-ins that only supported RGBA instead of BRGA pixel value order and 
multi-threaded with static (pictures or a generator) inputs.

Fixed: Vegas will now call IsIdentityAction before trying to render.

Fixed: setting the description property of a plug-in only worked in describeInContext and not
in the describe funtion.

Fixed: error opening a project if the generator is missing.

Fixed: if processing interlaced frames, and the plug-in's input and outputs are set to single 
or double, the plug-in will now be called twice (once at the frame and once at the half frame).

Fixed: clip instance missing unmapped properties.

Fixed: a bug with interpolating values in Fast, Slow, Sharp, or Smooth mode if the begin and 
end values were the same.

Scripting support added for OFX plug-ins.

Plug-in information cached to speed up load time of Vegas if the plug-in has not changed since 
last load.






Changes for Vegas Pro 10.0a Release
-----------------------------------

Fixed: Custom HWND UI pages with buttons or edit boxes will hang Vegas.

Fixed: Generators paying attention to alpha channel.

Fixed: Cannot request string value of a choice parameter.

Fixed: General effects with names "Foreground" or "Background" also acceptable input clip names 
for Vegas Compositors.

Fixed: Temporal frame callback offset for post-crop Event FX.

Fixed: Temporal frame callbacks results for post-crop Event FX when source media frame aspect 
does not match project.


Known Issues with Vegas Pro 10.0a Release
-----------------------------------------

kOfxPropVersionLabel on Host is not supported. (Use kOfxPropAPIVersion to get version number).

Vegas 10.0a will try to render on multiple cores with multiple instances of your plug-in if
your plug-in's kOfxImageEffectPluginRenderThreadSafety is set to kOfxImageEffectRenderInstanceSafe 
or kOfxImageEffectRenderFullySafe. Vegas does not call SyncPrivateData when this copy happens,
so if you depend on SyncPrivateData to save custom information, this will cause problems. To
work around set kOfxImageEffectPluginRenderThreadSafety to kOfxImageEffectRenderUnsafe.

Hidden parameters if animated show up on the timeline.

ParamDefine does not return kOfxStatErrExists if a parameter with that name exists already.

Presets are not saved in a language invariant way. Presets which were saved on a system set 
for using commas as the decimal separator, the presets will loose the fractional part of the 
number.

kOfxImageEffectActionGetClipPreferences action is not called to get the 
kOfxImageEffectFrameVarying flag setting for the output clip. If you have a generator that
changes over time without any explicit animation, Vegas does not request new renders for each
frame.

kOfxImageEffectPropFrameStep always returns 1.0, even in the case where it is stepping by 
half frames.

Effects set for multithreaded (kOfxImageEffectPluginRenderThreadSafety is set to 
kOfxImageEffectRenderInstanceSafe or kOfxImageEffectRenderFullySafe) and use the default OFX
pixel order of RGBA instead of the Vegas extension for BGRA pixel order and their inputs
are static (pictures or generator) then they might receive buffers with bad pixel ordering.

Crash if loading a project with ofx generators that were uninstalled or missing.





Changes for Vegas Pro 10.0 Final Release
----------------------------------------

Fixed custom and string parameters.

Fixed time issue where some times were in seconds and not frames.

Fixed Effect Duration property on Effect instance from always returning 10.


Known Issues with Vegas Pro 10.0 Final Release
----------------------------------------------

Custom HWND UI pages with buttons or edit boxes will hang Vegas. 

Can not individually access separate fields of interlaced frames. Temporal frame callbacks cannot
request separate fields of interlaced frames.

Temporal callbacks can return incorrect frames (offset in time) when used in the Event or 
Transition context with interlaced project settings in Good or Best mode.

Alpha channel in generators is ignored.

Cannot request string value of a choice parameter (workaround: request index instead).





Resources
---------

The OFX Web site
http://openfx.sourceforge.net/

The OFX API Programming Reference
http://openfx.sourceforge.net/Documentation/index.html

The OFX 1.3 API Programming Reference
http://openfx.sourceforge.net/Documentation/1.3/index.html
