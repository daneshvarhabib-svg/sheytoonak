// The Telegram side of TelegramWeb.cs. Telegram.WebApp comes from telegram-web-app.js, loaded by the page template.
mergeInto(LibraryManager.library, {
  SheytoonakTgHaptic: function (strength) {
    try {
      var w = window.Telegram && window.Telegram.WebApp;
      if (w && w.HapticFeedback) w.HapticFeedback.impactOccurred(strength >= 2 ? 'heavy' : strength == 1 ? 'medium' : 'light');
      else if (navigator.vibrate) navigator.vibrate(strength >= 2 ? 60 : 25);
    } catch (e) {}
  },
  SheytoonakTgInside: function () {
    try { var w = window.Telegram && window.Telegram.WebApp; return w && w.initData ? 1 : 0; } catch (e) { return 0; }
  }
});
