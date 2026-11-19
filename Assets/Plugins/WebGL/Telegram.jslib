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
  },
  SheytoonakTgStartParam: function () {
    var s = '';
    try { var w = window.Telegram && window.Telegram.WebApp; s = (w && w.initDataUnsafe && w.initDataUnsafe.start_param) || ''; } catch (e) {}
    var n = lengthBytesUTF8(s) + 1, p = _malloc(n); stringToUTF8(s, p, n); return p;
  },
  SheytoonakTgUserId: function () {
    var s = '';
    try { var w = window.Telegram && window.Telegram.WebApp; var u = w && w.initDataUnsafe && w.initDataUnsafe.user; s = u && u.id ? String(u.id) : ''; } catch (e) {}
    var n = lengthBytesUTF8(s) + 1, p = _malloc(n); stringToUTF8(s, p, n); return p;
  },
  SheytoonakTgShareLink: function (url, text) {
    try {
      var u = UTF8ToString(url), t = UTF8ToString(text);
      var share = 'https://t.me/share/url?url=' + encodeURIComponent(u) + '&text=' + encodeURIComponent(t);
      var w = window.Telegram && window.Telegram.WebApp;
      if (w && w.openTelegramLink) w.openTelegramLink(share); else window.open(share, '_blank');
    } catch (e) {}
  },
  SheytoonakTgShareStory: function (text, link, linkName) {
    try {
      var w = window.Telegram && window.Telegram.WebApp;
      if (!w || !w.shareToStory || !w.isVersionAtLeast || !w.isVersionAtLeast('7.8')) return 0;
      // the picture must be a public https address: story.jpg is published next to the page (WebGLTemplates/Telegram)
      var media = new URL('story.jpg', window.location.href).href;
      w.shareToStory(media, { text: UTF8ToString(text), widget_link: { url: UTF8ToString(link), name: UTF8ToString(linkName) } });
      return 1;
    } catch (e) { return 0; }
  },
  // Telegram Stars: our small server (server/stars) makes the invoice link with the bot's token; Telegram's own sheet charges.
  // The answer is polled by SheytoonakTgPayState: 0 waiting, 1 paid, 2 not paid.
  SheytoonakTgPay: function (backend, sku) {
    try {
      var w = window.Telegram && window.Telegram.WebApp;
      window.__sheyPay = 0;
      if (!w || !w.initData || !w.openInvoice) { window.__sheyPay = 2; return; }
      var url = UTF8ToString(backend) + '/invoice?sku=' + encodeURIComponent(UTF8ToString(sku)) + '&init=' + encodeURIComponent(w.initData);
      fetch(url).then(function (r) { return r.json(); }).then(function (j) {
        if (!j || !j.link) { window.__sheyPay = 2; return; }
        w.openInvoice(j.link, function (status) { window.__sheyPay = status === 'paid' ? 1 : 2; });
      }).catch(function () { window.__sheyPay = 2; });
    } catch (e) { window.__sheyPay = 2; }
  },
  SheytoonakTgPayState: function () { return window.__sheyPay | 0; }
});
