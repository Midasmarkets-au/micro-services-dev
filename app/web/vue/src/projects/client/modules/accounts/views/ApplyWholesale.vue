<template>
  <div v-if="isLoading" class="card-body">Loading</div>
  <div v-else class="apply-wholesale-group">
    <!-- ===================================================== process -->
    <!-- ===================================================== process -->
    <div
      class="card apply-wholesale-process"
      :style="{
        'border-bottom': currentStepIndex === 5 && 'none !important',
        'border-bottom-right-radius': currentStepIndex === 5 && '0 !important',
        'border-bottom-left-radius': currentStepIndex === 5 && '0 !important',
      }"
    >
      <div class="card-body">
        <div
          class="stepper stepper-links"
          id="kt_create_account_stepper"
          ref="horizontalWizardRef"
        >
          <div class="stepper-nav py-5">
            <div
              class="stepper-item"
              :class="{
                last: index == steps.length - 1,
                current: index === currentStepIndex,
              }"
              data-kt-stepper-element="nav"
              v-for="(item, index) in steps"
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
                style="cursor: pointer"
              >
                {{
                  {
                    started: $t("title.started"),
                    channel: $t("title.channel"),
                    supplement: $t("title.supplement"),
                    declaration: $t("title.declaration"),
                    finished: $t("title.finished"),
                  }[item]
                }}
              </h3>
              <div class="line" v-if="index < steps.length - 1"></div>
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

    <!-- ===================================================== process -->
    <!-- ===================================================== process -->
    <form
      class="mx-auto w-100 pb-10"
      :class="{
        'pt-5 ': currentStepIndex !== 5,
      }"
      @submit="handleSubmit"
    >
      <ApplicationStarted
        v-if="steps[currentStepIndex] === 'started'"
        ref="proStarted"
        :data="proApplicationFormData.started"
        :accountUid="currentAccount.uid"
        :account="currentAccount"
        @saved="handleSaved"
        @hasError="handleHasError"
      ></ApplicationStarted>

      <SophisticatedInvestorTest
        ref="proChannel"
        v-if="
          steps[currentStepIndex] === 'channel' &&
          proApplicationFormData.started.method == 2
        "
        :data="proApplicationFormData.channel"
        @saved="handleSaved"
        @hasError="handleHasError"
      ></SophisticatedInvestorTest>

      <ChannelOneSupplement
        v-if="
          steps[currentStepIndex] === 'supplement' &&
          proApplicationFormData.channel.channel == 1
        "
        ref="proSupplement"
        :accountUid="currentAccount.uid"
        :account="currentAccount"
        @saved="handleSaved"
        @hasError="handleHasError"
      ></ChannelOneSupplement>

      <ChannelTwoSupplement
        v-if="
          steps[currentStepIndex] === 'supplement' &&
          proApplicationFormData.channel.channel == 2
        "
        ref="proSupplement"
        :data="proApplicationFormData.supplement"
        @saved="handleSaved"
        @hasError="handleHasError"
      ></ChannelTwoSupplement>

      <ChannelThreeSupplement
        v-if="
          steps[currentStepIndex] === 'supplement' &&
          proApplicationFormData.channel.channel == 3
        "
        ref="proSupplement"
        :data="proApplicationFormData.supplement"
        @saved="handleSaved"
        @hasError="handleHasError"
      ></ChannelThreeSupplement>

      <RiskDeclaration
        ref="proDeclaration"
        v-if="steps[currentStepIndex] === 'declaration'"
        :data="proApplicationFormData"
        :accountUid="currentAccount.uid"
        :account="currentAccount"
        @saved="handleSaved"
        @hasError="handleHasError"
      ></RiskDeclaration>

      <ApplicationFinished
        v-if="steps[currentStepIndex] === 'finished'"
        :selectedMethod="proApplicationFormData.started.method"
      ></ApplicationFinished>

      <div v-if="currentStepIndex < 5" class="text-end mt-5">
        <div class="mr-2 d-flex gap-2 justify-content-end">
          <button
            type="button"
            class="btn btn-lg btn-light btn-radius d-flex align-items-center"
            v-if="currentStepIndex > 0 && currentStepIndex != 4"
            @click="previousStep"
          >
            <span class="svg-icon svg-icon-4 me-1">
              <inline-svg src="/images/icons/arrows/arr063.svg" />
            </span>
            {{ $t("action.back") }}
          </button>

          <button
            type="button"
            class="btn btn-lg btn-light btn-radius d-flex d-flex align-items-center"
            v-if="currentStepIndex === totalSteps - 1"
            @click="backHome"
          >
            <span class="indicator-label">{{ $t("action.backToHome") }}</span>
          </button>

          <button
            v-else
            type="submit"
            ref="submitButton"
            class="btn btn-lg btn-primary btn-radius"
          >
            <span class="indicator-label" v-if="currentStepIndex == 0">
              {{ $t("action.applyForUpgrade") }}
            </span>

            <span
              v-else
              class="indicator-label d-flex d-flex align-items-center"
            >
              {{ $t("action.next") }}
              <span class="svg-icon svg-icon-3 ms-2 me-0">
                <inline-svg src="/images/icons/arrows/arr064.svg" />
              </span>
            </span>

            <span class="indicator-progress">
              {{ $t("action.pleaseWait") }}
              <span
                class="spinner-border spinner-border-sm align-middle ms-2"
              ></span>
            </span>
          </button>
        </div>
      </div>
    </form>
  </div>
</template>

<script lang="ts" setup>
import { useRoute } from "vue-router";
import { ref, onMounted, computed } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "../services/AccountService";
import RiskDeclaration from "../components/wholesale/RiskDeclaration.vue";
import ApplicationStarted from "../components/wholesale/ApplicationStarted.vue";
import ApplicationFinished from "../components/wholesale/ApplicationFinished.vue";
import ChannelOneSupplement from "../components/wholesale/ChannelOneSupplement.vue";
import ChannelTwoSupplement from "../components/wholesale/ChannelTwoSupplement.vue";
import ChannelThreeSupplement from "../components/wholesale/ChannelThreeSupplement.vue";
import SophisticatedInvestorTest from "../components/wholesale/SophisticatedInvestorTest.vue";

const steps = ref([
  "started",
  "channel",
  "supplement",
  "declaration",
  "finished",
]);

const proStarted = ref(null);
const proChannel = ref(null);
const proSupplement = ref(null);
const proDeclaration = ref(null);

const refs = {
  started: proStarted,
  channel: proChannel,
  supplement: proSupplement,
  declaration: proDeclaration,
};
const route = useRoute();
const totalSteps = ref(5);
const isLoading = ref(false);
const currentStepIndex = ref(0);
const currentAccount = ref({} as any);
const accountsList = ref(Array<any>());
const proApplicationFormData = ref<any>({});
const submitButton = ref<HTMLButtonElement | null>(null);

const progress = computed(() => {
  if (currentStepIndex.value === 5) return 100;
  return ((currentStepIndex.value + 1) / totalSteps.value) * 100;
});

onMounted(() => {
  fetchAccountInfo();
});

const backHome = () => {
  window.location.href = "/";
};

const fetchAccountInfo = async () => {
  isLoading.value = true;

  try {
    const res = await AccountService.queryAccounts({
      hasTradeAccount: true,
    });
    accountsList.value = res.data;
    currentAccount.value = getCurrentAccountFromUrlParams();
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const getCurrentAccountFromUrlParams = () =>
  accountsList.value.find(
    (item) => item.tradeAccount?.accountNumber == route.params.accountNumber
  );

const previousStep = () => {
  if (currentStepIndex.value > 0) {
    currentStepIndex.value--;
    window.scrollTo(0, 0);
  }
};

const handleSubmit = async (e) => {
  e.preventDefault();
  if (submitButton.value) {
    // eslint-disable-next-line
    submitButton.value!.disabled = true;
    // Activate indicator
    submitButton.value.setAttribute("data-kt-indicator", "on");
  }
  await refs[steps.value[currentStepIndex.value]].value?.handleStepSubmit();
};

const handleSaved = async (step: number, data?: any) => {
  if (data) proApplicationFormData.value[steps.value[step - 1]] = data;

  if (step == 3 && data.wrongAns > 1) {
    try {
      await AccountService.postWholesaleApplication({
        accountUid: currentAccount.value.uid,
        accountNumber: currentAccount.value.tradeAccount.accountNumber,
        request: proApplicationFormData.value,
      });
      window.location.href = "/";
    } catch (error) {
      MsgPrompt.error(error);
    }
  } else {
    submitButton.value?.removeAttribute("data-kt-indicator");
    // eslint-disable-next-line
    submitButton.value!.disabled = false;

    if (proApplicationFormData.value.started.method == 1) {
      currentStepIndex.value = 4;
    } else {
      currentStepIndex.value++;
    }

    window.scrollTo(0, 0);
  }
};

const handleHasError = () => {
  //Deactivate indicator
  submitButton.value?.removeAttribute("data-kt-indicator");
  // eslint-disable-next-line
  submitButton.value!.disabled = false;
};
</script>

<style lang="scss" scoped>
.apply-wholesale-group {
  .apply-wholesale-process {
    @media (max-width: 768px) {
      display: none;
    }
  }
}
</style>
