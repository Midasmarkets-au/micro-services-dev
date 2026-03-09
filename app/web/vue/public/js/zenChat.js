// // Start of Zendesk Chat Script

// window.ChatActivateFlag = false;

// window.$zopim ||
//   (function (d, s) {
//     var z = ($zopim = function (c) {
//         z._.push(c);
//       }),
//       $ = (z.s = d.createElement(s)),
//       e = d.getElementsByTagName(s)[0];
//     z.set = function (o) {
//       z.set._.push(o);
//     };
//     z._ = [];
//     z.set._ = [];
//     $.async = !0;
//     $.setAttribute("charset", "utf-8");
//     $.src = "https://v2.zopim.com/?5Q8DrxhrMaBtU5w84go2yS6qbXj1MSFK";
//     z.t = +new Date();
//     $.type = "text/javascript";
//     e.parentNode.insertBefore($, e);
//   })(document, "script");

// $zopim(function () {
//   var languageMap = {
//     "en-us": "en",
//     "zh-cn": "zh-cn",
//     "zh-hk": "zh-tw",
//     "zh-tw": "zh-tw",
//     "vi-vn": "vi",
//     "th-th": "th",
//     "jp-jp": "ja",
//     // "mn-mn": "en",
//     "id-id": "id",
//     "ms-my": "ms",
//   };

//   var lang = languageMap[localStorage.getItem("language")] || "en";

//   $zopim.livechat.setLanguage(lang);
//   $zopim.livechat.window.setOffsetHorizontal(105);
//   $zopim.livechat.hideAll();

//   $zopim.livechat.window.onShow(() => {
//     ChatActivateFlag = true;
//   });

//   $zopim.livechat.window.onHide(() => {
//     ChatActivateFlag = false;
//     $zopim.livechat.window.hide();
//   });
// });

// $.async = !0;
// $.setAttribute("charset", "utf-8");
// $.src = "https://v2.zopim.com/?5Q8DrxhrMaBtU5w84go2yS6qbXj1MSFK";
// z.t = +new Date();
// $.type = "text/javascript";
// e.parentNode.insertBefore($, e);

var isOpen = false;
zE("messenger", "close");
var app_language = "<?php echo $app_language; ?>";
function normalizeLocale(locale) {
  switch (locale.toLowerCase()) {
    case "cn":
    case "zh_cn":
      return "zh-CN";
    case "tw":
    case "zh_tw":
      return "zh-TW";
    case "en_us":
      return "en-US";
    default:
      return "en"; // 或 'zh-CN' 作为默认
  }
}
app_language = normalizeLocale(app_language);
console.log("app_language", app_language);
zE("messenger:set", "locale", app_language);
$(document).ready(function () {
  $("#zendesk").click(function () {
    if (isOpen) {
      isOpen = false;
      zE("messenger", "close");
    } else {
      isOpen = true;
      zE("messenger", "open");
    }
  });
});
