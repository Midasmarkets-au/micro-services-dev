<template>
  <div>
    <!-- <el-switch
      class="mb-9"
      v-model="rebateType"
      width="80"
      size="large"
      inline-prompt
      style="--el-switch-on-color: #0053ad; --el-switch-off-color: #b1b1b1"
      :active-text="'Advance'"
      :inactive-text="'General'"
    />

    <div v-if="!rebateType">
      <label class="fs-5 fw-semibold me-3">Rebate: </label>
      <el-input class="w-60px me-3" type="number" v-model="rebatePercent" />
      %
      <div class="mt-9">
        <button class="btn btn-primary btn-sm" @click="confirmBoxRef?.show()">
          {{ $t("action.submit") }}
        </button>
      </div>
    </div> -->

    <div>
      <!--------------------------------------------------------- step 1-->
      <div>
        <span class="fw-500 fs-2 me-5">step 1</span>
        <span
          >Select the type of account that the subordinate IB is allowed to open
          (multiple selections possible)</span
        >

        <!--------------------------------------------------------- step 1 options-->
        <div class="mt-5 ms-5">
          <input
            id="rebateStdAccount"
            class="form-check-input widget-9-check me-3"
            type="checkbox"
            name="stdAccount"
            v-model="selectStdAccount"
            value="Standard"
          />
          <label class="me-9" for="rebateStdAccount">Standard Account</label>
          <input
            id="rebateAdvAccount"
            class="form-check-input widget-9-check me-3"
            type="checkbox"
            name="advAccount"
            v-model="selectAdvAccount"
            value="Advanced"
          />
          <label class="me-9" for="rebateAdvAccount">Advanced Account</label>
          <input
            id="rebateAlphaAccount"
            class="form-check-input widget-9-check me-3"
            type="checkbox"
            name="alphaAccount"
            v-model="selectAlphaAccount"
            value="Alpha"
          />
          <label class="me-9" for="rebateAlphaAccount">Raw Account</label>
        </div>
      </div>

      <!--------------------------------------------------------- step 2 -->
      <div class="mt-9">
        <span class="fw-500 fs-2 me-5">step 2</span>
        <span>Set the rebate</span>

        <!--------------------------------------------------------- step 2 Options -->
        <div v-if="selectStdAccount">
          <div
            class="d-flex align-items-center mt-9 mb-3"
            @click="showStdForm = !showStdForm"
            style="cursor: pointer"
          >
            <div
              class="vertical-line"
              style="
                border-left: 3px solid #800020;
                height: 16px;
                margin-right: 15px;
              "
            ></div>
            <div class="fw-500 fs-4">Standard Account</div>
            <span
              class="arrow svg-icon svg-icon-7 ms-5"
              :class="{ 'rotate-up': showStdForm }"
            >
              <div class="svg-container">
                <inline-svg src="/images/icons/arrows/down001.svg" />
              </div>
            </span>
          </div>
          <StandardAccountRebateForm
            :class="showStdForm ? 'showForm' : 'hideForm'"
            :rebateTemporaryDate="rebateTemporaryDate"
          />
        </div>

        <div v-if="selectAdvAccount">
          <div
            class="d-flex align-items-center mt-9 mb-3"
            @click="showAdvForm = !showAdvForm"
            style="cursor: pointer"
          >
            <div
              class="vertical-line"
              style="
                border-left: 3px solid #800020;
                height: 16px;
                margin-right: 15px;
              "
            ></div>
            <div class="fw-500 fs-4">Advance Account</div>
            <span
              class="arrow svg-icon svg-icon-7 ms-5"
              :class="{ 'rotate-up': showAdvForm }"
            >
              <div class="svg-container">
                <inline-svg src="/images/icons/arrows/down001.svg" />
              </div>
            </span>
          </div>
          <AdvanceAccountRebateForm
            :class="showAdvForm ? 'showForm' : 'hideForm'"
            :rebateTemporaryDate="rebateTemporaryDate"
          />
        </div>

        <div v-if="selectAlphaAccount">
          <div
            class="d-flex align-items-center mt-9 mb-3"
            @click="showAllPlatform = !showAllPlatform"
            style="cursor: pointer"
          >
            <div
              class="vertical-line"
              style="
                border-left: 3px solid #800020;
                height: 16px;
                margin-right: 15px;
              "
            ></div>
            <div class="fw-500 fs-4">Raw Account</div>
            <span
              class="arrow svg-icon svg-icon-7 ms-5"
              :class="{ 'rotate-up': showAllPlatform }"
            >
              <div class="svg-container">
                <inline-svg src="/images/icons/arrows/down001.svg" />
              </div>
            </span>
          </div>
          <AlphaAccountRebateForm
            :class="showAllPlatform ? 'showForm' : 'hideForm'"
            :rebateTemporaryDate="rebateTemporaryDate"
          />
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import svc from "../services/IbService";
import { useStore } from "@/store";
import { useRoute } from "vue-router";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import StandardAccountRebateForm from "./form/StandardAccountRebateForm.vue";
import AdvanceAccountRebateForm from "./form/AdvantageAccountRebateForm.vue";
import AlphaAccountRebateForm from "./form/AlphaAccountRebateForm.vue";

const showAllPlatform = ref(false);
const showAdvForm = ref(false);
const showStdForm = ref(false);
// const rebateType = ref(false);
const isLoading = ref(true);
const store = useStore();
const route = useRoute();
const rebateRules = ref(Array<any>());
const selectAlphaAccount = ref(false);
const selectAdvAccount = ref(false);
const selectStdAccount = ref(false);

const accountUid = ref(-1);

const agentAccount = ref(store.state.AgentModule.agentAccount);
watch(
  () => store.state.AgentModule.agentAccount,
  (newVal) => {
    agentAccount.value = newVal;
    if (accountUid.value !== -1) fetchData(1);
  }
);

defineProps<{
  accountRole: AccountRoleTypes;
}>();

const criteria = ref({
  page: 1,
  size: 10,
  sourceAccountUid: accountUid.value,
  targetAccountUid: agentAccount.value.accountUid,
});

watch(
  () => route.params.accountId,
  (newVal) => {
    accountUid.value = parseInt((newVal as string) || "-1");
    // console.log(accountUid.value);
    criteria.value.sourceAccountUid = accountUid.value;
    if (accountUid.value !== -1) fetchData(1);
  }
);

const fetchData = async (selectedPage: number) => {
  isLoading.value = true;
  criteria.value.page = selectedPage;
  try {
    const res = await svc.queryRebateDistributionRulesOfAgent(criteria.value);
    // console.log(res.criteria);
    rebateRules.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    // console.log(error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  accountUid.value = parseInt((route.params.accountId as string) || "-1");
  fetchData(1);
});

const rebateTemporaryDate = ref([
  {
    name: "Forex",
    total: 10,
    personal: 0,
    remain: 0,
    p: 0,
    c: 0,
  },
  {
    name: "Commission",
    total: 10,
    personal: 0,
    remain: 0,
    p: 0,
    c: 0,
  },
  {
    name: "index",
    total: 10,
    personal: 0,
    remain: 0,
    p: 0,
    c: 0,
  },
]);
</script>
<style scoped lang="scss">
.svg-container {
  transition: transform 0.3s ease-in-out;
}

.arrow.rotate-up .svg-container {
  transform: rotate(-180deg);
}

.showForm {
  max-height: 1000px;
  transition: max-height 0.3s ease-in-out;
}

.hideForm {
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.3s ease-in-out;
}
</style>
