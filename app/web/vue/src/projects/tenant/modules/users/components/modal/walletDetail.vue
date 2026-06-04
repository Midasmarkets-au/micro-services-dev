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
    <table class="table align-middle table-row-dashed fs-6 gy-5">
      <thead>
        <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
          <th>ID</th>
          <th>Matter ID</th>
          <th>Prev Balance</th>
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
          <td>{{ item.id }}</td>
          <td>{{ item.matterId }}</td>
          <td><BalanceShow :currency-id="currencyId" :balance="item.prevBalance" /></td>
          <td><BalanceShow :currency-id="currencyId" :balance="item.amount" /></td>
          <td><TimeShow :date-iso-string="item.createdOn" type="inFields" /></td>
        </tr>
      </tbody>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </table>
  </SiderDetail2>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SiderDetail2 from "@/components/SiderDetail2.vue";
import TableFooter from "@/components/TableFooter.vue";
import { useI18n } from "vue-i18n";
import axios from "axios";

const t = useI18n().t;
const isLoading = ref(true);
const submitted = ref(false);

const walletDetailShowRef = ref<any>();
const detailTitle = ref(t("fields.walletDetail"));
const walletDetail = ref<any[]>([]);
const currencyId = ref(0);
const walletId = ref(0);

const criteria = ref({ page: 1, size: 20, total: 0 } as any);

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
    const res = await axios.get(
      `/api/v1/tenant/wallet/${walletId.value}/transaction-k8s`,
      { params: { page: _page, size: criteria.value.size } }
    );
    walletDetail.value = res.data.data ?? [];
    criteria.value = { ...criteria.value, ...res.data.criteria };
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
};

const submit = () => {};
const close = () => { walletDetailShowRef.value?.hide(); };

defineExpose({ show });
</script>
