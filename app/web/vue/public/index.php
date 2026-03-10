<!DOCTYPE html>
<!--
Use below html tag for RTL version
<html lang="en" dir="rtl" direction="rtl" style="direction: rtl">
-->
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta
      name="viewport"
      content="width=device-width, initial-scale=1, shrink-to-fit=no, maximum-scale=1"
    />
    <!--    <meta-->
    <!--      name="viewport"-->
    <!--      content="width=device-width, initial-scale=1, shrink-to-fit=no"-->
    <!--    />-->
    <meta name="keywords" content="" />
    <?php
      $host = parse_url($url, PHP_URL_HOST);
      if($host == 'jp.thebcr.com'){
        echo '<!--jp--><meta name="site" content="jp" />';
      }else{
        echo '<!--bcr--><meta name="site" content="MidasMarkets" />';
      }
    ?>
    <meta name="description" content="" />
    <title>MidasMarkets</title>
    <link rel="icon" href="/image/favicon.ico" />
    <link
      rel="stylesheet"
      href="https://fonts.googleapis.com/css?family=Inter:300,400,500,600,700"
    />
    <link rel="stylesheet" href="<%= BASE_URL %>fonticon/fonticon.css" />
    <link rel="stylesheet" href="<%= BASE_URL %>splash-screen.css" />
    <title><%= htmlWebpackPlugin.options.title %></title>

    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link
      href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&display=swap"
      rel="stylesheet"
    />
    <script
      src="https://code.jquery.com/jquery-3.7.1.min.js"
      integrity="sha256-/JqT3SQfawRcv/BIHPThkBvs0OEvtFFmqPF/lYI/Cxo="
      crossorigin="anonymous"
    ></script>
    <script id="ze-snippet" src="https://static.zdassets.com/ekr/snippet.js?key=fdf56cc0-f9e1-4dc6-b7ba-7e46cee980b2">
    </script>
  </head>
  <body class="page-loading">
    <!--begin::Theme mode setup on page load-->
    <script>
      window["api"] = "";
      window["apiv2"] = "";
      if (document.documentElement) {
        var defaultThemeMode = "dark";

        // var name = document.body.getAttribute("data-kt-name");
        // var themeMode = localStorage.getItem(
        //   "kt_" + (name ? name + "_" : "") + "theme_mode_value"
        // );

        // if (themeMode === null) {
        //   if (defaultThemeMode === "system") {
        //     themeMode = window.matchMedia("(prefers-color-scheme: dark)")
        //       .matches
        //       ? "dark"
        //       : "light";
        //   } else {
        //     themeMode = defaultThemeMode;
        //   }
        // }
        // // console.log("themeMode", themeMode);
        // document.documentElement.setAttribute("data-theme", themeMode);
      }
    </script>
    <!--end::Theme mode setup on page load-->
    <noscript>
      <strong
        >We're sorry but bcr doesn't work properly without JavaScript enabled.
        Please enable it to continue.</strong
      >
    </noscript>
    <div id="app"></div>

    <!-- built files will be auto injected -->
    <!--begin::Loading markup-->
    <div id="splash-screen" class="splash-screen">
      <?php
      if($host == 'jp.thebcr.com'){
        echo '<!--jp--><img src="<%= BASE_URL %>images/BCR-Logo JP-White.gif" alt="TheBCR" />';
      }else{
        echo '<!--bcr--><img src="<%= BASE_URL %>images/BCR-Logo OG-White.gif" alt="TheBCR" />';
      }
      ?>
    </div>
    <script></script>
    <!-- Google tag (gtag.js) -->
    <!-- <script async src="https://www.googletagmanager.com/gtag/js?id=G-P7ZRDF005B"></script>
<script>
  window.dataLayer = window.dataLayer || [];
  const gtag = () => {dataLayer.push(arguments);}
  gtag('js', new Date());

  gtag('config', 'G-P7ZRDF005B');
  console.log(gtag)
</script> -->
    <!--end::Loading markup-->
  </body>
</html>
