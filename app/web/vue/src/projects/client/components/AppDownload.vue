<template>
  <div class="card card-flush mb-5">
    <div class="card-header">
      <div class="card-title">
        <h3 class="d-flex align-items-center mx-1 m-0 fw-bold">
          {{ $t("title.bcrAppDownload") }}
        </h3>
      </div>
    </div>
    <!-- <div class="card-body pt-0 pb-5">
      <div class="fs-6 d-flex justify-content-between align-items-center">
        <div class="guide-box">
          <a
            href="https://apps.apple.com/app/bcr-trader/id6740502137"
            target="_blank"
          >
            <img
              :width="isMobile ? 120 : 150"
              src="/images/download/app-store-white.png"
          /></a>
        </div>
        <div class="guide-box">
          <a href="/apk/bcr_trader_1_0_17_1.apk"
            ><img
              :width="isMobile ? 120 : 150"
              src="/images/download/android-apk.png"
          /></a>
        </div>
      </div>
    </div> -->
    <div class="card-body pt-0 pb-2 mt-1">
      <div class="fs-6 d-flex gap-3">
        <div class="guide-box btn btn-light-primary">
          <div class="d-flex align-items-center gap-1">
            <!-- <a
              href="https://apps.apple.com/app/bcr-trader/id6740502137"
              target="_blank"
              class="d-flex justify-content-between align-items-center gap-2"
            > -->
            <a
              href="https://download.mql5.com/cdn/mobile/mt5/ios?server=MMCo-Main"
              target="_blank"
              class="d-flex justify-content-between align-items-center gap-2"
            >
              <SvgIcon name="ios" path="app" />
              <div>
                <div class="fs-10">Download on the</div>
                <div class="fs-6 fw-semibold">APP STORE</div>
              </div>
            </a>
          </div>
        </div>
        <div class="guide-box btn btn-light-primary">
          <div class="d-flex align-items-center gap-1">
            <!-- <a
              href="/apk/bcr_trader_1_0_17_1.apk"
              target="_blank"
              class="d-flex justify-content-between align-items-center gap-2"
            > -->
            <a
              href="https://download.midasmkts.com/download/metatrader5.apk"
              target="_blank"
              class="d-flex justify-content-between align-items-center gap-2"
            >
              <SvgIcon name="android" path="app" />
              <div>
                <div class="fs-10">Download on the</div>
                <div class="fs-6 fw-semibold">Android APK</div>
              </div>
            </a>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { isMobile } from "@/core/config/WindowConfig";
import { is } from "@vee-validate/rules";
import SvgIcon from "./SvgIcon.vue";
</script>

<style scoped>
.guide-box {
  height: 5.285rem;
  background-color: #fff;
  box-shadow: 0 8px 10px rgba(63, 119, 212, 0.3);
  border: 1px solid #fff;
  align-items: center;
  border-radius: 8px;
  padding: 1.23rem;
  flex: 1;
  margin-bottom: 1.43rem;
  cursor: pointer;
}
</style>
