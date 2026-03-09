<template>
  <el-dialog
    v-model="dialogRef"
    :title="title"
    width="1400"
    align-center
    :before-close="reset"
    style="max-height: 90vh"
  >
    <div class="overflow-auto" style="max-height: 70vh">
      <table class="table table-tenant table-hover">
        <thead>
          <tr>
            <td>{{ $t("fields.transactionHashId") }}</td>
            <td>{{ $t("fields.paymentId") }}</td>
            <td>{{ $t("fields.paymentNo") }}</td>
            <td>{{ $t("fields.amount") }}</td>
            <td>{{ $t("fields.status") }}</td>
            <td>{{ $t("fields.fromAddress") }}</td>
            <td>{{ $t("fields.createdOn") }}</td>
            <td>{{ $t("fields.user") }}</td>
          </tr>
        </thead>
        <tbody v-if="isLoading" style="height: 300px">
          <tr>
            <td colspan="12">
              <scale-loader :color="'#ffc730'"></scale-loader>
            </td>
          </tr>
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr
            v-for="item in data"
            :key="item.id"
            :class="{
              'tr-select': item.id === accountSelected,
            }"
            @click="selectedAccount(item.id)"
          >
            <td>
              <div class="d-flex align-items-center">
                <div class="width-control">
                  {{ item.transactionHash }}
                </div>
                <TinyCopyBox
                  :val="item.transactionHash.toString()"
                ></TinyCopyBox>
              </div>
            </td>
            <td>{{ item.paymentId }}</td>
            <td>
              <div v-if="item.inUsePayment">
                {{ item.inUsePayment.paymentNumber }}
              </div>
            </td>
            <td><BalanceShow :balance="item.amount" /></td>
            <td>{{ getStatusLabel(item.status) }}</td>
            <td>
              <div class="d-flex align-items-center">
                <div class="width-control">
                  {{ item.fromAddress }}
                </div>
                <TinyCopyBox
                  :val="item.transactionHash.toString()"
                ></TinyCopyBox>
              </div>
            </td>

            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <div v-if="item.inUsePayment">
                <UserInfo :user="item.inUsePayment.user" class="me-2" />
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <TableFooter @page-change="fetchData" :criteria="criteria" />
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.close")
        }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import PaymentService from "../../services/PaymentService";
import { CryptoTransactionStatusOptions } from "@/core/types/crypto/CryptoStatus";
import TinyCopyBox from "@/components/TinyCopyBox.vue";
const dialogRef = ref(false);
const data = ref<any>([]);
const isLoading = ref(false);
const title = ref("");

const criteria = ref<any>({
  page: 1,
  size: 25,
  cryptoId: 0,
  sortField: "createdOn",
  sortFlag: true,
});
const show = (item: any) => {
  dialogRef.value = true;
  title.value = item.name;
  criteria.value.cryptoId = item.id;
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  try {
    criteria.value.page = _page;
    const res = await PaymentService.queryCryptoTransactions(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const reset = () => {
  dialogRef.value = false;
  data.value = [];
  title.value = "";
  criteria.value = {
    page: 1,
    size: 25,
    cryptoId: 0,
    sortField: "createdOn",
    sortFlag: true,
  };
};

const getStatusLabel = (value: number) => {
  const option = CryptoTransactionStatusOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};

const accountSelected = ref(0);

const selectedAccount = (id: number) => {
  accountSelected.value = id;
};
defineExpose({
  show,
});
</script>
<style scoped>
.width-control {
  display: inline-block;
  max-width: 100px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-right: 5px;
}
</style>
