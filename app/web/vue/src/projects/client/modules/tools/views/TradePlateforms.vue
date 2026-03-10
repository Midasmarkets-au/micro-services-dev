<template>
  <div :class="{ card: !isMobile, 'pt-5': !isMobile, 'mt-10': !isMobile }">
    <div class="card-body" :class="!isMobile ? 'pt-10' : ''">
      <div class="row">
        <div v-if="mt4Enabled" class="col-12 col-md-6 p-1 pt-15 p-md-15">
          <div class="card download-card">
            <div class="card-header position-relative border-0">
              <div class="download-platform-icon">
                <img
                  src="/images/icons/brand/mt4.svg"
                  width="100"
                  height="100"
                />
                <h3>{{ $t("tip.mt4") }}</h3>
              </div>
            </div>
            <div class="card-body min-h-650px">
              <h4>{{ $t("tip.desktop") }}</h4>
              <div class="flex">
                <div class="download-mobile">
                  <div class="flex flex-row align-items-center">
                    <img width="42" src="/images/download/mac.png" />
                    <label
                      ><a
                        :href="platformDownloadLinks.bvi.mt4.mac"
                        target="_blank"
                        >{{ $t("tip.download") }}</a
                      >
                      {{ $t("tip.downloadForMac") }}</label
                    >
                  </div>
                </div>
                <div class="download-mobile">
                  <div class="flex flex-row align-items-center">
                    <img width="42" src="/images/download/windows.png" /><label
                      ><a
                        :href="platformDownloadLinks.bvi.mt4.windows"
                        target="_blank"
                        >{{ $t("tip.download") }}</a
                      >
                      {{ $t("tip.downloadForWindows") }}</label
                    >
                  </div>
                </div>
              </div>
              <h4>{{ $t("tip.mobileAndTablet") }}</h4>
              <div class="flex">
                <div class="download-mobile gap-5">
                  <div class="flex flex-row align-items-center">
                    <img
                      width="42"
                      height="42"
                      src="/images/download/ios.png"
                    />
                    <label
                      ><a
                        :href="platformDownloadLinks.bvi.mt4.ios"
                        target="_blank"
                        >{{ $t("tip.download") }}</a
                      >
                      {{ $t("tip.downloadForIos")
                      }}{{ $t("tip.orScanCode") }}</label
                    >
                    <!-- <img width="88" src="/images/download/ios-bcr-new.png" /> -->
                  </div>
                  <img :src="qrCodeSrcs.mt4Iso" width="88" />
                </div>
                <div class="download-mobile gap-5">
                  <div class="flex flex-row align-items-center">
                    <img
                      width="42"
                      height="42"
                      src="/images/download/android.png"
                    />
                    <label
                      ><a
                        :href="platformDownloadLinks.bvi.mt4.android"
                        target="_blank"
                        >{{ $t("tip.download") }}</a
                      >
                      {{ $t("tip.downloadForAndroid")
                      }}{{ $t("tip.orScanCode") }}</label
                    >
                  </div>
                  <!-- <img width="88" src="/images/download/android-bcr-new.png" /> -->
                  <img :src="qrCodeSrcs.mt4Android" width="88" />
                </div>
              </div>
              <template v-if="webTraderEnabled"
                ><h4>{{ $t("tip.webTrader") }}</h4>
                <div class="download-web">
                  <label>
                    <img
                      style="width: 175px"
                      src="/images/download/browsers.jpeg"
                    />
                  </label>
                  <router-link
                    to="/webtrader"
                    class="btn btn-sm btn-primary btn-radius"
                    >{{ $t("tip.visit") }}</router-link
                  >
                </div></template
              >
              <p class="mt-2" v-if="region != 'au'">
                {{ $t("tip.mt5Policy") }}
              </p>
            </div>
          </div>
        </div>

        <div
          v-if="mt5Enabled"
          :class="!isMobile ? 'pt-20' : 'px-6'"
          class="col-12 col-md-6 p-1 mt-15 mt-md-0 p-md-15"
        >
          <div class="card download-card">
            <div class="card-header position-relative border-0">
              <div class="download-platform-icon">
                <img
                  src="/images/icons/brand/mt5.svg"
                  width="100"
                  height="100"
                />
                <h3>{{ $t("tip.mt5") }}</h3>
              </div>
            </div>

            <div class="card-body min-h-650px">
              <h4>{{ $t("tip.desktop") }}</h4>
              <div class="flex">
                <div class="download-mobile">
                  <div class="flex flex-row align-items-center">
                    <img width="42" src="/images/download/mac.png" />
                    <label
                      ><a
                        :href="platformDownloadLinks.bvi.mt5.mac"
                        style="color: #0a46aa"
                        >{{ $t("tip.download") }}</a
                      >
                      {{ $t("tip.downloadForMac") }}</label
                    >
                  </div>
                </div>
                <div class="download-mobile">
                  <div class="flex flex-row align-items-center">
                    <img width="42" src="/images/download/windows.png" /><label
                      ><a
                        :href="platformDownloadLinks.bvi.mt5.windows"
                        style="color: #0a46aa"
                        >{{ $t("tip.download") }}</a
                      >
                      {{ $t("tip.downloadForWindows") }}</label
                    >
                  </div>
                </div>
              </div>
              <h4>{{ $t("tip.mobileAndTablet") }}</h4>
              <div class="flex">
                <div class="download-mobile gap-5">
                  <div class="flex flex-row align-items-center">
                    <img
                      width="42"
                      height="42"
                      src="/images/download/ios.png"
                    />
                    <label
                      ><a
                        :href="platformDownloadLinks.bvi.mt5.ios"
                        style="color: #0a46aa"
                        >{{ $t("tip.download") }}</a
                      >
                      {{ $t("tip.downloadForIos") }}
                      {{ $t("tip.orScanCode") }}</label
                    >
                  </div>
                  <!-- <img width="88" src="/images/download/mt5-ios.png" /> -->
                  <img width="88" :src="qrCodeSrcs.mt5Iso" />
                </div>
                <div class="download-mobile gap-5">
                  <div class="flex flex-row align-items-center">
                    <img
                      width="42"
                      height="42"
                      src="/images/download/android.png"
                    />
                    <label
                      ><a
                        :href="platformDownloadLinks.bvi.mt5.android"
                        style="color: #0a46aa"
                        >{{ $t("tip.download") }}</a
                      >
                      {{ $t("tip.downloadForAndroid") }}
                      {{ $t("tip.orScanCode") }}</label
                    >
                  </div>
                  <!-- <img width="88" src="/images/download/mt5-android.png" /> -->
                  <img width="88" :src="qrCodeSrcs.mt5Android" />
                </div>
              </div>
              <template v-if="webTraderEnabled"
                ><h4>{{ $t("tip.webTrader") }}</h4>
                <div class="download-web">
                  <label>
                    <img
                      style="width: 175px"
                      src="/images/download/browsers.jpeg"
                    />
                  </label>
                  <!-- <router-link
                    to="/webtrader5"
                    class="btn btn-sm btn-primary btn-radius"
                    >{{ $t("tip.visit") }}</router-link
                  > -->
                </div></template
              >
              <p class="mt-2" v-if="region != 'au'">
                {{ $t("tip.mt5Policy") }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import "../assets/css/style.css";
import { useStore } from "@/store";
import { ref, computed, onMounted } from "vue";
import { PlatformTypes } from "@/core/types/ServiceTypes";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { isMobile } from "@/core/config/WindowConfig";
import { platformDownloadLinks } from "@/core/data/platformDownloadLinks";
import { generateQrCodeImageSrc } from "@/core/utils/qrCodeUtils";

const store = useStore();
const user = store.state.AuthModule.user;
const region = ref(user.tenancy);
const qrCodeSrcs = ref({
  mt4Iso: "",
  mt5Iso: "",
  mt4Android: "",
  mt5Android: "",
});

const webTraderEnabled = computed(() => {
  const projectConfig: PublicSetting = store.state.AuthModule.config;
  return projectConfig?.webTraderEnabled;
});
const mt4Enabled = computed(() => {
  const projectConfig: PublicSetting = store.state.AuthModule.config;

  return (
    projectConfig?.tradingPlatformAvailable.includes(
      PlatformTypes.MetaTrader4
    ) ||
    projectConfig?.tradingPlatformAvailable.includes(10) ||
    projectConfig?.tradingPlatformAvailable.includes(
      PlatformTypes.MetaTrader4Co
    )
  );
});

const getQrCodeSrc = async (url: string) => {
  var srcUrl = await generateQrCodeImageSrc(url);
  return srcUrl;
};

const mt5Enabled = computed(() => {
  const projectConfig: PublicSetting = store.state.AuthModule.config;
  return projectConfig?.tradingPlatformAvailable.includes(
    PlatformTypes.MetaTrader5
  );
});

onMounted(async () => {
  if (mt4Enabled.value) {
    qrCodeSrcs.value.mt4Android = await getQrCodeSrc(
      platformDownloadLinks.bvi.mt4.android
    );
    qrCodeSrcs.value.mt4Iso = await getQrCodeSrc(
      platformDownloadLinks.bvi.mt4.ios
    );
  }
  if (mt5Enabled.value) {
    qrCodeSrcs.value.mt5Android = await getQrCodeSrc(
      platformDownloadLinks.bvi.mt5.android
    );
    qrCodeSrcs.value.mt5Iso = await getQrCodeSrc(
      platformDownloadLinks.bvi.mt5.ios
    );
  }
});
</script>
<style scoped lang="scss">
.disable-trader {
  opacity: 0.5;
  pointer-events: none;
  h1 {
    font-size: 7rem;
    color: #ccc;
  }
}

.download-card {
  border: 0 !important;
  border-radius: 10px;
  box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);
}

.download-card .download-platform-icon {
  text-align: center;
  position: absolute;
  top: -50px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}

.download-card .card-header {
  height: 120px;
}
.download-card .download-platform-icon h3 {
  line-height: 40px;
  font-size: 28px;
  padding: 15px 0;
  color: rgba(77, 77, 77, 1);
}

.download-card h4 {
  color: rgba(30, 40, 51, 1);
  padding: 15px 0;
  font-weight: 500;
  font-size: 20px;
  line-height: 28px;
}

.download-card .download-mobile {
  margin-bottom: 1.25rem;
  align-items: center;
  width: 100%;
  display: flex;
  justify-content: center;
  flex-direction: column;
}

.download-card .download-mobile img {
  margin-right: 1.25rem;
  flex-shrink: 0;
}

.download-card .download-mobile label {
  flex-grow: 1;
}

.download-card .download-web {
  margin-bottom: 1.25rem;
  padding-right: 1.25rem;
  align-items: center;
  width: 100%;
  display: inline-flex;
}
.download-card .download-web label {
  flex-grow: 1;
}

.download-card .download-web a {
  margin-left: 1.25rem;
  flex-shrink: 0;
}
</style>
