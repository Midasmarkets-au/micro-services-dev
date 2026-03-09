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
    <iframe frameborder="0" :src="webTraderUrl" width="100%" height="100%">
    </iframe>
  </div>
  <!--end::Row-->
</template>

<script lang="ts" setup>
import { computed, onMounted } from "vue";
import "../assets/css/style.css";
import { useI18n } from "vue-i18n";
import { getLanguage } from "@/core/types/LanguageTypes";
import { ServiceTypes } from "@/core/types/ServiceTypes";
import { useRoute } from "vue-router";

const i18n = useI18n();
const route = useRoute();

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

const getServiceId = () => {
  const serviceId = route.params.serviceId;
  return Array.isArray(serviceId) ? serviceId[0] : serviceId;
};

const webTraderUrl = computed(() => {
  const serviceId = parseInt(getServiceId());
  if (serviceId == ServiceTypes.MetaTrader4Co) {
    return `https://metatraderweb.app/trade?servers=MMCo-Main&lang=${currentLanguage.value}&login=${route.params.accountNumber}&trade_server=MMCo-Main`;
  } else if (serviceId == ServiceTypes.MetaTrader4) {
    return `https://metatraderweb.app/trade?servers=MMCo-Main&lang=${currentLanguage.value}&login=${route.params.accountNumber}&trade_server=MMCo-Main`;
  }
  return `https://metatraderweb.app/trade?servers=MMCo-Main&amp;trade_server=MMCo-Main&amp;lang=${currentLanguage.value}`;
});
</script>
