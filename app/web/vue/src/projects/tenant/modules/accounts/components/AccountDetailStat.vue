<template>
  <div>
    <div class="card">
      <div
        class="card-header d-flex justify-content-between align-items-center"
      >
        <div class="card-title">
          <div>Funding Statistic</div>
          <div>
            <span class="badge badge-primary ms-3 me-3"> Deposit </span
            ><span class="badge badge-warning me-3 text-black"> Withdraw </span
            ><span class="badge badge-success me-3"> Positive Net </span
            ><span class="badge badge-danger me-3"> Negative Net </span
            ><span class="badge badge-secondary me-3"> Rebate </span>
          </div>
        </div>

        <div>
          <div class="d-flex justify-content-between">
            <div class="d-flex gap-3">
              <el-date-picker
                class="w-250px"
                v-model="period"
                type="daterange"
                :start-placeholder="$t('fields.startDate')"
                :end-placeholder="$t('fields.endDate')"
                :default-time="defaultTime"
              />

              <el-button plain type="primary" @click="confirmSearch">{{
                $t("action.search")
              }}</el-button>
              <el-button plain type="info" @click="clearSearchFilterCriteria">{{
                $t("action.clear")
              }}</el-button>
            </div>

            <div class="d-flex gap-3"></div>
          </div>
        </div>
      </div>

      <div class="card-body" v-if="refresh">
        <AccountStat
          ref="accountStatRef"
          :uid="$props.accountDetails.uid"
          :from="criteria.from"
          :to="criteria.to"
          :viewClient="criteria.viewClient"
        ></AccountStat>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { watch, onMounted, ref } from "vue";
import AccountStat from "./AccountStat.vue";
import moment from "moment";

const period = ref([] as any);
const refresh = ref(true);
const accountStatRef = ref<InstanceType<typeof AccountStat>>();
const props = defineProps<{
  accountDetails: any;
}>();

const criteria = ref<any>({
  uid: props.accountDetails.uid,
  from: "",
  to: "",
  viewClient: false,
});

const defaultTime = ref<[Date, Date]>([
  new Date(2000, 1, 1, 0, 0, 0),
  new Date(2000, 2, 1, 23, 59, 59),
]);

const confirmSearch = () => {
  refresh.value = false;
  setTimeout(() => {
    refresh.value = true;
  }, 100);
};

const clearSearchFilterCriteria = () => {
  criteria.value = { from: "", to: "", viewClient: false };
  period.value = [];
  refresh.value = false;
  setTimeout(() => {
    refresh.value = true;
  }, 100);
};

onMounted(() => {
  console.log("mounted: accountDetails");
});

watch(
  () => period.value,
  (periodVal) => {
    if (periodVal && periodVal.length > 0 && typeof periodVal[0] !== "string") {
      periodVal = [
        moment(periodVal[0]).local().toISOString(),
        moment(periodVal[1]).local().toISOString(),
      ];
    }
    criteria.value.from = periodVal ? periodVal[0] : null;
    criteria.value.to = periodVal ? periodVal[1] : null;
  }
);
</script>
