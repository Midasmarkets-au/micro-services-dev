<template>
  <div
    v-if="!isMobile"
    class="stepper stepper-pills stepper-column"
    style="
      min-width: 200px;
      border-right: 1px solid #e4e6ef;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
    "
  >
    <div class="stepper-nav ps-0">
      <!--begin::Step 1-->
      <div
        class="stepper-item"
        :class="{
          ['current']: currentStep === 1,
        }"
      >
        <div class="stepper-wrapper">
          <div class="stepper-icon w-40px h-40px stepper-icon-round">
            <span class="stepper-number">1</span>
          </div>
          <div class="stepper-label">
            <div>{{ $t("title.depositChannel") }}</div>
          </div>
        </div>
        <div class="h-40px"></div>
      </div>
      <!--end::Step 1-->

      <!--begin::Step 2-->
      <div
        class="stepper-item"
        :class="{
          ['current']: currentStep === 2,
        }"
      >
        <div class="stepper-wrapper">
          <div class="stepper-icon w-40px h-40px stepper-icon-round">
            <span class="stepper-number">2</span>
          </div>
          <div class="stepper-label">
            <div>{{ $t("title.importantNote") }}</div>
          </div>
        </div>
        <div class="h-40px"></div>
      </div>
      <!--end::Step 2-->

      <!--begin::Step 3-->
      <div
        class="stepper-item"
        :class="{
          ['current']: currentStep === 3,
        }"
      >
        <div class="stepper-wrapper">
          <div class="stepper-icon w-40px h-40px stepper-icon-round">
            <span class="stepper-number">3</span>
          </div>
          <div class="stepper-label">
            <div>{{ $t("title.fillRequiredInfo") }}</div>
          </div>
        </div>
        <div class="h-40px"></div>
      </div>
      <!--end::Step 3-->

      <!--begin::Step 4-->
      <div
        class="stepper-item"
        :class="{
          ['current']: currentStep === 4,
        }"
      >
        <div class="stepper-wrapper">
          <div class="stepper-icon w-40px h-40px stepper-icon-round">
            <span class="stepper-number">4</span>
          </div>
          <div class="stepper-label">
            <div>{{ $t("title.verifyDetails") }}</div>
          </div>
        </div>
        <div class="h-40px"></div>
      </div>
      <!--end::Step 4-->

      <!--begin::Step 5-->
      <div
        class="stepper-item"
        :class="{
          ['current']: currentStep === 5,
        }"
      >
        <div class="stepper-wrapper">
          <div class="stepper-icon w-40px h-40px stepper-icon-round">
            <span class="stepper-number">5</span>
          </div>
          <div class="stepper-label">
            <div>{{ $t("title.instruction") }}</div>
          </div>
        </div>
      </div>
      <!--end::Step 5-->
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
const currentStep = inject("currentStep");
</script>
<style lang="scss" scoped>
.stepper-icon-round {
  border-radius: 14px;
}
</style>
