<template>
  <!--begin::Row-->
  <div
    id="webterminal"
    style="
      width: 100vw;
      height: 100vh;
      border: 3px solid gray;
      position: absolute;
      left: 0;
    "
  >
    <iframe
      frameborder="0"
      :src="webTraderUrl"
      width="100%"
      height="100%"
    ></iframe>
  </div>
  <!--end::Row-->
</template>

<script lang="ts" setup>
import { computed, onMounted } from "vue";
import "../assets/css/style.css";
import { useI18n } from "vue-i18n";
import { getLanguage } from "@/core/types/LanguageTypes";
import { useRoute } from "vue-router";

const route = useRoute();
const i18n = useI18n();

onMounted(() => {
  // let metaTrader = document.createElement("script");
  // metaTrader.setAttribute("src", "https://trade.mql5.com/trade/widget.js");
  // document.head.appendChild(metaTrader);
  // metaTrader.addEventListener("load", () => {
  //   var webTrader = document.createElement("script");
  //   webTrader.type = "text/javascript";
  //   webTrader.text =
  //     'new MetaTraderWebTerminal( "webterminal", { version: 4, servers: ["BCRCo-DEMO","BCRCo-REAL"], server: "BCRCo-DEMO", utmCampaign: "", utmSource: "cfds.thebcr.com", startMode: "no_autologin", language: "en", colorScheme: "black_on_white" } ); ';
  //   document.head.appendChild(webTrader);
  // });
  // console.log(document);
});
const currentLanguage = computed(() => {
  switch (getLanguage.value) {
    case "en-us":
      return "en";
    case "zh-cn":
      return "zh";
    case "zh-hk":
      return "zt";
    case "vi-vn":
      return "vi";
    case "th-th":
      return "th";
    case "jp-jp":
      return "ja";
    case "ms-my":
      return "ms";
    default:
      return "en";
  }
});

const webTraderUrl = computed(() => {
  if (route.params.accountNumber) {
    return `https://mt5.midasmkts.com/terminal?lang=${currentLanguage.value}&server=MMCo-Main&login=${route.params.accountNumber}`;
  }
  return `https://mt5.midasmkts.com/terminal?lang=${currentLanguage.value}&server=MMCo-Main`;
});
</script>

<style>
.webtraderContent {
  height: 100%;
}
</style>
