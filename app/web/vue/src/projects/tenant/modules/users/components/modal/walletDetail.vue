<template>
  <SiderDetail2
    :save="submit"
    :discard="close"
    :title="detailTitle"
    elId="user_detail_show"
    :isLoading="isLoading"
    :submited="submitted"
    :isDisabled="false"
    :savingTitle="$t('action.saving')"
    :show-footer="false"
    width="1200px"
    ref="walletDetailShowRef"
  >
    <!-- <div class="d-flex align-items-center gap-4">
      <el-select class="w-200px" v-model="criteria.matterType">
        <el-option
          v-for="(item, index) in types"
          :key="index"
          :label="index"
          :value="item"
        ></el-option>
      </el-select>
      <div class="w-400px">
        <el-date-picker
          v-model="period"
          type="daterange"
          range-separator="To"
          start-placeholder="Start date"
          end-placeholder="End date"
        />
      </div>
      <el-button type="primary" @click="submit">{{
        $t("action.search")
      }}</el-button>
      <el-button type="info" @click="reset">{{ $t("action.clear") }}</el-button>
    </div> -->
    <table class="table align-middle table-row-dashed fs-6 gy-5">
      <thead>
        <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
          <th>{{ $t("fields.type") }}</th>
          <th>{{ $t("fields.status") }}</th>
          <th>{{ $t("fields.amount") }}</th>
          <th>{{ $t("fields.time") }}</th>
        </tr>
      </thead>

      <tbody v-if="isLoading">
        <LoadingRing />
      </tbody>
      <tbody v-else-if="!isLoading && walletDetail.length === 0">
        <NoDataBox />
      </tbody>
      <tbody v-else>
        <tr v-for="(item, index) in walletDetail" :key="index">
          <td>{{ $t(`type.matter.${item.matter.type}`) }}</td>
          <td>
            {{ $t(`type.transactionState.${item.matter.stateId}`) }}
          </td>
          <td>
            <BalanceShow :currency-id="currencyId" :balance="item.amount" />
          </td>
          <td>
            <TimeShow :date-iso-string="item.matter.postedOn" type="inFields" />
          </td>
        </tr>
      </tbody>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </table>
  </SiderDetail2>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SiderDetail from "@/components/SiderDetail.vue";
import SiderDetail2 from "@/components/SiderDetail2.vue";
import UserService from "../../services/UserService";
import TableFooter from "@/components/TableFooter.vue";
import { useI18n } from "vue-i18n";

const t = useI18n().t;
const isLoading = ref(true);
const submitted = ref(false);

const walletDetailShowRef = ref<InstanceType<typeof SiderDetail>>();
const detailTitle = ref(t("fields.walletDetail"));
const walletDetail = ref(Array<any>());
const currencyId = ref(0);
const walletId = ref(0);
const period = ref([] as any);
const types = {
  System: 0,
  InternalTransfer: 200,
  Deposit: 300,
  Withdrawal: 400,
  Rebate: 500,
};

const criteria = ref({
  page: 1,
  size: 10,
  matterType: null,
  from: null,
  to: null,
} as any);

const show = (_walletId: number, _currencyId: number) => {
  walletDetailShowRef.value?.show();
  currencyId.value = _currencyId;
  walletId.value = _walletId;
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;

  try {
    criteria.value.page = _page;
    const res = await UserService.getWalletTransactionByWalletId(
      walletId.value,
      criteria.value
    );
    criteria.value = res.criteria;
    walletDetail.value = res.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const submit = () => {
  criteria.value.from = period.value[0].toISOString();
  criteria.value.to = period.value[1].toISOString();
  fetchData(1);
};

const reset = () => {
  criteria.value.matterType = null;
  criteria.value.from = null;
  criteria.value.to = null;
  period.value = [];
  fetchData(1);
};
const close = () => {
  walletDetailShowRef.value?.hide();
};
const getStatus = (object, value) => {
  return Object.keys(object).find((key) => object[key] === value);
};
defineExpose({ show });
</script>

<style></style>
