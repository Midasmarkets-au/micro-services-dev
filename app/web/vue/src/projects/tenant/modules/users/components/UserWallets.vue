<template>
  <div>
    <div class="card">
      <div class="card-header">
        <div class="card-title">{{ $t("title.userWallets") }}</div>
      </div>

      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">id</th>
              <th class="">{{ $t("fields.currency") }}</th>
              <th class="">{{ $t("fields.fundType") }}</th>
              <th class="">{{ $t("fields.amount") }}</th>
              <th class="">{{ $t("fields.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && userWallets.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in userWallets" :key="index">
              <td>{{ item.id }}</td>
              <td>{{ $t(`type.currency.${item.currencyId}`) }}</td>
              <td>{{ $t(`type.fundType.${item.fundType}`) }}</td>
              <td>
                <BalanceShow
                  :currency-id="item.currencyId"
                  :balance="item.balance"
                />
              </td>
              <td>
                <el-button
                  @click="viewWalletTransactions(item.id, item.currencyId)"
                  >{{ $t(`action.viewTransactions`) }}</el-button
                >

                <el-button @click="getPaymentMethods(item.id)"
                  >Set Payment Methods</el-button
                >
              </td>
            </tr>
          </tbody>
        </table>

        <TableFooter @page-change="getWallets" :criteria="criteria" />
      </div>
    </div>

    <div v-if="paymentMethodsWalletId != 0" class="card mt-9">
      <div class="card-header">
        <div
          class="card-title d-flex align-item-center justify-content-between"
          style="width: 100%"
        >
          <div>
            <div class="d-flex align-item-center fs-3 fw-semobold">
              Wallet Withdraw Methods ( ID: {{ paymentMethodsWalletId }} )
            </div>
          </div>
          <div>
            <el-switch
              v-model="hideActive"
              active-text="Show Active Only"
              inactive-text="Show All"
              inactive-color="#13ce66"
              class="me-5"
              :disabled="isLoading"
            ></el-switch>
          </div>
        </div>
      </div>

      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>{{ $t("fields.name") }}</th>
              <th>{{ $t("fields.serviceActive") }}</th>
              <th>Active</th>
            </tr>
          </thead>

          <tbody v-if="gettingPaymentMethod">
            <LoadingRing />
          </tbody>

          <tbody
            v-else
            v-for="(value, i) in walletPaymentServices"
            :key="i"
            class="fw-semibold tbodyBorder"
          >
            <tr>
              <td colspan="9" class="fw-semibold pt-7 pb-0">
                <div class="d-flex justify-content-between">
                  <h4>{{ i }}</h4>
                </div>
              </td>
            </tr>

            <tr v-for="(item, index) in value" :key="index">
              <td class="">
                <span
                  class="p-3"
                  :class="item.status ? '' : 'bg-warning rounded'"
                  >{{ item.name }}</span
                >
              </td>
              <td class="p-0">
                <label
                  class="form-check form-switch form-switch-sm form-check-custom form-check-solid"
                >
                  <input
                    class="form-check-input center-switch"
                    name="activate"
                    type="checkbox"
                    :checked="item.status"
                    :disabled="true"
                  />
                  <span class="form-check-label fw-semobold text-muted"></span>
                </label>
              </td>
              <td class="p-0">
                <el-switch
                  v-if="item.status"
                  v-model="item.accessStatus"
                  :active-value="10"
                  :inactive-value="0"
                  class="me-5"
                  :disabled="isLoading"
                  @change="paymentMethodChanged(item)"
                ></el-switch>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
  <walletDetail ref="walletDetailRef" />
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { ElNotification } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TableFooter from "@/components/TableFooter.vue";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import walletDetail from "@/projects/tenant/modules/users/components/modal/walletDetail.vue";

const props = defineProps<{
  partyId: number;
}>();

const isLoading = ref(true);
const hideActive = ref(false);
const deposit = ref({} as any);
const withdraw = ref({} as any);
const paymentMethodsWalletId = ref(0);
const userWallets = ref(Array<any>());
const gettingPaymentMethod = ref(false);
const walletPaymentMethods = ref({} as any);
const walletPaymentServices = ref(Array<any>());
const walletDetailRef = ref<InstanceType<typeof walletDetail>>();

const criteria = ref({
  partyId: props.partyId,
  page: 1,
  size: 10,
} as any);

const getWallets = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;

  try {
    const responseBody = await TenantGlobalService.queryWallets(criteria.value);
    userWallets.value = responseBody.data;
    criteria.value = responseBody.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const viewWalletTransactions = (walletId: number, currencyId: number) => {
  walletDetailRef.value?.show(walletId, currencyId);
};

const paymentMethodChanged = async (_item: any) => {
  try {
    if (_item.accessStatus) {
      await PaymentService.putEnableWalletPaymentMethodById(
        _item.id,
        paymentMethodsWalletId.value
      );
    } else {
      await PaymentService.putDisableWalletPaymentMethodById(
        _item.id,
        paymentMethodsWalletId.value
      );
    }

    ElNotification({
      title: _item.name,
      message: "Payment method updated successfully",
      type: "success",
    });
  } catch (e) {
    ElNotification.error({
      title: "Error",
      message: "Error Submit",
      type: "error",
    });
    console.error(e);
  } finally {
    isLoading.value = false;
  }
};

const getPaymentMethods = async (_id) => {
  gettingPaymentMethod.value = true;
  paymentMethodsWalletId.value = _id;

  try {
    walletPaymentMethods.value =
      await PaymentService.getWalletPaymentMethodById(_id);

    var tempDeposit = walletPaymentMethods.value.deposit;
    tempDeposit = tempDeposit.reduce((index, item) => {
      if (index[item.group]) {
        index[item.group].push(item);
      } else {
        index[item.group] = [item];
      }
      return index;
    }, {});
    Object.keys(tempDeposit)
      .sort()
      .forEach((group) => {
        deposit.value[group] = tempDeposit[group];
      });

    var tempwithdrawal = walletPaymentMethods.value.withdrawal;
    tempwithdrawal = tempwithdrawal.reduce((index, item) => {
      if (index[item.group]) {
        index[item.group].push(item);
      } else {
        index[item.group] = [item];
      }
      return index;
    }, {});

    Object.keys(tempwithdrawal)
      .sort()
      .forEach((group) => {
        withdraw.value[group] = tempwithdrawal[group];
      });

    walletPaymentServices.value = withdraw.value;
    gettingPaymentMethod.value = false;
  } catch (error) {
    MsgPrompt.error(error);
  }
};

watch(hideActive, (value) => {
  if (value) {
    walletPaymentServices.value = Object.keys(walletPaymentServices.value)
      .filter((group) => {
        return walletPaymentServices.value[group].some((item) => {
          return item.status == 10;
        });
      })
      .reduce((index, group) => {
        index[group] = walletPaymentServices.value[group].filter((item) => {
          return item.status == 10;
        });
        return index;
      }, {});
  } else {
    getPaymentMethods(paymentMethodsWalletId.value);
  }
});

onMounted(() => {
  getWallets(1);
  isLoading.value = false;
});
</script>

<style scoped></style>
'
