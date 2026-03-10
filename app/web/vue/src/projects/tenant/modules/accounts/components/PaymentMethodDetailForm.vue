<template>
  <div id="paymentMethodDetailForm" class="card mb-5 mx-auto">
    <div class="card-header">
      <div
        class="card-title d-flex align-item-center justify-content-between"
        style="width: 100%"
      >
        <div>
          <ul
            class="d-flex align-item-center nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-3 fw-semobold"
          >
            <li class="nav-item">
              <a
                :class="[
                  'nav-link text-active-primary pb-1',
                  { active: currTab === 'Deposit' },
                ]"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab('Deposit')"
                >Deposit</a
              >
            </li>
            <li class="nav-item">
              <a
                :class="[
                  'nav-link text-active-primary pb-1',
                  { active: currTab === 'Withdraw' },
                ]"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab('Withdraw')"
                >Withdraw</a
              >
            </li>
          </ul>
        </div>
        <div>
          <el-switch
            v-model="hideActive"
            active-text="Show Active Only"
            inactive-text="Show All"
            inactive-color="#13ce66"
            class="me-5"
            :disabled="isLoading"
            @change="hideShowActive"
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

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && accountPaymentServices.length === 0">
          <NoDataBox />
        </tbody>

        <tbody
          v-else
          v-for="(value, key) in accountPaymentServices"
          :key="key"
          class="fw-semibold tbodyBorder"
        >
          <tr>
            <td colspan="9" class="fw-semibold pt-7 pb-0">
              <div class="d-flex justify-content-between">
                <h4>{{ key }}</h4>
                <div>
                  <el-button
                    type="success"
                    @click="enableGroupAll(String(key))"
                    :disabled="isLoading || isSubmitting"
                    size="small"
                  >
                    Enable All
                  </el-button>
                  <el-button
                    type="danger"
                    @click="disableGroupAll(String(key))"
                    :disabled="isLoading || isSubmitting"
                    size="small"
                  >
                    Disable All
                  </el-button>
                </div>
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

    <div v-if="props.openForm" class="d-flex justify-content-end me-11">
      <el-form-item>
        <el-button type="primary" @click="submitForm">Submit</el-button>
      </el-form-item>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ElNotification } from "element-plus";
import { ref, onMounted, inject } from "vue";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";

const isLoading = ref(true);
const isSubmitting = ref(false);
const deposit = ref({} as any);
const hideActive = ref(true);
const currTab = ref("Deposit");
const withdraw = ref({} as any);
const accountPaymentMethods = ref({} as any);
const accountPaymentServices = ref(Array<any>() as any);
const accountDetails = inject<any>(AccountInjectionKeys.ACCOUNT_DETAILS);

const emits = defineEmits<{
  (e: "submit"): void;
}>();

const props = defineProps({
  openForm: { type: Boolean, required: false, default: false },
});

const enableGroupAll = async (group: string) => {
  isSubmitting.value = true;
  try {
    await PaymentService.putEnableAccountPaymentMethodByGroup(
      group,
      accountDetails.value.id
    );
    accountPaymentServices.value[group].forEach((item: any) => {
      item.accessStatus = 10;
    });
    ElNotification({
      title: group,
      message: "All Deposit enabled successfully",
      type: "success",
    });
    // fetchData();
  } catch (e) {
    ElNotification.error({
      title: "Error",
      message: "Error Submit",
      type: "error",
    });
    console.error(e);
  } finally {
    isSubmitting.value = false;
  }
};

const disableGroupAll = async (group: string) => {
  isSubmitting.value = true;
  try {
    await PaymentService.putDisableAccountPaymentMethodByGroup(
      group,
      accountDetails.value.id
    );
    accountPaymentServices.value[group].forEach((item: any) => {
      item.accessStatus = 0;
    });
    ElNotification({
      title: group,
      message: "All Deposit disabled successfully",
      type: "success",
    });
    // fetchData();
  } catch (e) {
    ElNotification.error({
      title: "Error",
      message: "Error Submit",
      type: "error",
    });
    console.error(e);
  } finally {
    isSubmitting.value = false;
  }
};

const paymentMethodChanged = async (_item: any) => {
  try {
    if (_item.accessStatus) {
      await PaymentService.putEnableAccountPaymentMethodById(
        _item.id,
        accountDetails.value.id
      );
    } else {
      await PaymentService.putDisableAccountPaymentMethodById(
        _item.id,
        accountDetails.value.id
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

const changeTab = (tab: string) => {
  currTab.value = tab;
  hideActive.value = true;

  if (tab === "Deposit") {
    accountPaymentServices.value = deposit.value;
  } else {
    accountPaymentServices.value = withdraw.value;
  }

  hideShowActive();
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    accountPaymentMethods.value =
      await PaymentService.getAccountPaymentMethodById(accountDetails.value.id);

    var tempDeposit = accountPaymentMethods.value.deposit;
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

    var tempwithdrawal = accountPaymentMethods.value.withdrawal;
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

    if (currTab.value === "Deposit") {
      accountPaymentServices.value = deposit.value;
    } else {
      accountPaymentServices.value = withdraw.value;
    }

    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
};

const hideShowActive = () => {
  if (hideActive.value) {
    accountPaymentServices.value = Object.keys(accountPaymentServices.value)
      .filter((group) => {
        return accountPaymentServices.value[group].some((item) => {
          return item.status == 10;
        });
      })
      .reduce((index, group) => {
        index[group] = accountPaymentServices.value[group].filter((item) => {
          return item.status == 10;
        });
        return index;
      }, {});
  } else {
    fetchData();
  }
};

// watch(hideActive, (value) => {
//   if (value) {
//     accountPaymentServices.value = Object.keys(accountPaymentServices.value)
//       .filter((group) => {
//         return accountPaymentServices.value[group].some((item) => {
//           return item.status == 10;
//         });
//       })
//       .reduce((index, group) => {
//         index[group] = accountPaymentServices.value[group].filter((item) => {
//           return item.status == 10;
//         });
//         return index;
//       }, {});
//   } else {
//     fetchData();
//   }
// });

const submitForm = () => {
  emits("submit");
};

onMounted(async () => {
  await fetchData();
  hideShowActive();
  isLoading.value = false;
});
</script>

<style scoped>
#paymentMethodDetailForm .center-switch {
  margin: 0 auto;
}
.form-check {
  display: block;
}
.tbodyBorder {
  border-bottom: 1px solid #00000023 !important;
  margin-top: 20px !important;
}
</style>
