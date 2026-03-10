<template>
  <div
    v-if="!isMobile"
    class="card"
    :class="{ step_5: currentStepIndex === 5 }"
  >
    <div>
      <div
        class="stepper stepper-links"
        id="kt_create_account_stepper"
        ref="horizontalWizardRef"
      >
        <div class="stepper-nav py-7">
          <div
            class="stepper-item"
            :class="{
              last: index == items.settings.length - 1,
              current: index === currentStepIndex,
            }"
            data-kt-stepper-element="nav"
            v-for="(item, index) in items.settings"
            :key="index"
          >
            <div class="icon">
              <template v-if="index < currentStepIndex">
                <inline-svg
                  src="/images/verification/finished.svg"
                ></inline-svg>
              </template>
              <template v-else>
                {{ index + 1 }}
              </template>
            </div>
            <h3
              class="stepper-title"
              :class="{ 'text-dark': index === currentStepIndex }"
              @click="jumpStep(index)"
              style="cursor: pointer"
            >
              {{
                {
                  started: $t("title.started"),
                  info: $t("title.personalInfo"),
                  financial: $t("title.financial"),
                  quiz: $t("title.quiz"),
                  agreement: $t("title.agreement"),
                  document: $t("title.document"),
                }[item]
              }}
            </h3>
            <div class="line" v-if="index < items.settings.length - 1"></div>
          </div>
        </div>
        <!-- <div class="progress h-8px w-100">
          <div
            class="progress-bar bg-success"
            role="progressbar"
            :style="'width: ' + progress + '%'"
            :aria-valuenow="progress"
            aria-valuemin="0"
            aria-valuemax="100"
          ></div>
        </div> -->
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { isMobile } from "@/core/config/WindowConfig";
import { inject, computed } from "vue";
import Can from "@/core/plugins/ICan";
const currentStepIndex = inject<any>("currentStepIndex");
const totalSteps = inject<any>("totalSteps");
const items = inject<any>("items");

const progress = computed(() => {
  if (currentStepIndex.value === totalSteps.value) return 100;
  return ((currentStepIndex.value + 1) / totalSteps.value) * 100;
});

const jumpStep = (_index: number) => {
  if (!Can.can("SuperAdmin") && items.value.data?.status != 0) {
    return;
  }

  if (_index > currentStepIndex.value) {
    return;
  } else {
    currentStepIndex.value = _index;
  }
};
</script>
<style lang="css" scoped>
/* .step_5 {
  border-bottom: none !important;
  border-bottom-right-radius: 0 !important;
  border-bottom-left-radius: 0 !important;
} */
</style>
