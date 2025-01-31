var WXTouchLibrary = 
{
  $WXTouchManager: 
  {
    onTouchMove: null,
  }, 

  WXSetOnTouchMove: function (callback) {
    Module["WXTouchManager"] = WXTouchManager;
    WXTouchManager.onTouchMove = callback;
  },
};

autoAddDeps(WXTouchLibrary, '$WXTouchManager');
mergeInto(LibraryManager.library, WXTouchLibrary);