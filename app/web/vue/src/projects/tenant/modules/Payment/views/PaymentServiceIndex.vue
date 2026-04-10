<template>
  <div class="d-flex justify-content-between">
    <ul
      class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-3 fw-semobold mb-8"
    >
      <li class="nav-item">
        <a
          :class="[
            'nav-link text-active-primary pb-1',
            { active: tab == 'Deposit' },
            { 'disabled opacity-50 pe-none': isLoading },
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
            { active: tab == 'Withdrawal' },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="changeTab('Withdrawal')"
          >Withdraw</a
        >
      </li>
    </ul>

    <el-button type="warning" @click="resetCache">{{
      $t("action.resetCache")
    }}</el-button>
  </div>

  <div class="card mb-5 mb-xl-8">
    <div class="card-header">
      <div class="card-title">
        <el-switch
          v-model="hideActive"
          active-text="Hide Inactive"
          inactive-text="Show All"
          size="large"
          style="font-size: 18px"
          :disabled="isSubmitting"
        >
        </el-switch>
        <el-button
          style="margin-left: 10px"
          @click="showSortModal"
          :disabled="isSubmitting || isLoading"
        >
          <i class="fa-solid fa-sort me-2"></i>
          {{ $t("action.adjustClientDisplayOrder") }}
        </el-button>
      </div>
      <div class="card-toolbar gap-4">
        <div class="d-flex align-items-center">
          <div class="fs-5 fw-bold me-3">Callback Time ( Min. )</div>
          <el-input
            class="w-100px"
            v-model="callbackTime"
            placeholder=""
            type="number"
            :disabled="isSubmitting"
          />
          <el-button
            class="ms-3"
            type="success"
            @click="updateCallbackTime"
            :disabled="isSubmitting"
            plain
          >
            Update
          </el-button>
        </div>

        <el-button
          @click="showPaymentCreate(services)"
          type="primary"
          :disabled="isSubmitting"
        >
          Add Payment Service
        </el-button>
      </div>
    </div>

    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6"
        id="table_accounts_requests"
      >
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && services.length === 0">
          <NoDataBox />
        </tbody>

        <tbody
          v-else
          v-for="(value, i) in services"
          :key="i"
          class="fw-semibold text-gray-900 tbody"
        >
          <tr>
            <td v-if="i == 'Union Pay'" colspan="13" class="fw-semibold">
              <div class="d-flex justify-content-between align-items-center">
                <h4>{{ i }}</h4>
                <div class="d-flex align-items-center">
                  <div class="fs-5 fw-bold w-150px">High Dollar Threshold</div>
                  <el-input
                    class="w-150px ms-3"
                    type="number"
                    v-model="highDollarInfo.value"
                    :disabled="isSubmitting"
                  ></el-input>
                  <el-button
                    class="ms-3"
                    type="success"
                    @click="updateHighDollarConfigValue"
                    :disabled="isSubmitting"
                  >
                    Update
                  </el-button>
                  <div class="fs-5 fw-bold ms-11 me-3">Updated By:</div>
                  <div>
                    {{ highDollarInfo.staff }}
                  </div>
                </div>
              </div>
            </td>

            <td v-else colspan="9" class="fw-semibold mt-4">
              <h4>{{ i }}</h4>
            </td>
          </tr>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("fields.name") }}</th>
            <th>{{ $t("fields.autoDeposit") }}</th>
            <th>{{ $t("fields.activate") }}</th>
            <!-- <th>{{ $t("fields.deposit") }}</th>
            <th>{{ $t("fields.withdraw") }}</th> -->
            <th v-if="i == 'Union Pay'">{{ $t("fields.highDollar") }}</th>
            <th v-else></th>
            <th>{{ $t("fields.currency") }}</th>
            <th>{{ $t("fields.initMinAmount") }}</th>
            <th>{{ $t("fields.minAmount") }}</th>
            <th>{{ $t("fields.maxAmount") }}</th>
            <th v-if="i == 'Union Pay'">{{ $t("fields.percentage") }}</th>
            <th v-else></th>
            <th>{{ $t("fields.updatedBy") }}</th>
            <th>{{ $t("action.action") }}</th>
            <th>{{ $t("action.detail") }}</th>
          </tr>
          <tr v-for="(item, index) in value" :key="index">
            <td>
              <div class="d-flex align-items-center gap-1">
                <el-input type="text" style="width: 120px" v-model="item.name">
                </el-input>
                <span>{{ item.id }}</span>
              </div>
            </td>

            <td>
              <el-switch
                v-model="item.isAutoDepositEnabled"
                @change="singleSubmit(item)"
                :active-value="1"
                :inactive-value="0"
                :disabled="isSubmitting"
              ></el-switch>
            </td>

            <td>
              <el-switch
                v-model="item.status"
                @change="singleSubmit(item)"
                :disabled="isSubmitting"
              ></el-switch>
            </td>

            <td v-if="i == 'Union Pay'">
              <el-switch
                v-model="item.isHighDollarEnabled"
                @change="singleSubmit(item)"
                :active-value="1"
                :inactive-value="0"
              ></el-switch>
            </td>
            <td v-else></td>

            <td>{{ t(`type.currency.${item.currencyId}`) }}</td>
            <td>
              <el-input
                class="w-100"
                type="number"
                v-model="item.initialValue"
                :disabled="isSubmitting"
              >
                <template #prefix> $USD:&nbsp; </template>
              </el-input>
            </td>
            <td>
              <el-input
                v-model="item.minValue"
                :disabled="isSubmitting"
                class="w-100"
                type="number"
                ><template #prefix> $USD:&nbsp; </template></el-input
              >
            </td>
            <td>
              <el-input
                v-model="item.maxValue"
                :disabled="isSubmitting"
                class="w-100"
                type="number"
                ><template #prefix> $USD:&nbsp; </template></el-input
              >
            </td>

            <td v-if="i == 'Union Pay'">
              <div style="width: 120px">
                <el-input v-model="item.percentage" class="w-100" type="number"
                  ><template #append>%</template></el-input
                >
              </div>
            </td>
            <td v-else></td>

            <td>
              <el-popover v-if="updateHistory[item.id]" :width="350">
                <template #reference>{{
                  updateHistory[item.id]["userName"]
                }}</template>
                <template #default>
                  <div>
                    <p class="fw-bold text-warning-emphasis">
                      {{ $t("fields.updated_at") }}
                      {{ updateHistory[item.id]["createdOn"] }}
                    </p>
                  </div>
                  <div>
                    <p
                      v-for="(value, index) in updateHistory[item.id][
                        'previous'
                      ]"
                      :key="index"
                    >
                      <span v-if="value"
                        >{{ index }}: {{ value }} ⇒
                        {{ updateHistory[item.id]["current"][index] }}</span
                      >
                    </p>
                  </div>
                </template>
              </el-popover>
            </td>

            <td>
              <el-button
                type="warning"
                @click="singleSubmit(item)"
                :disabled="isSubmitting"
              >
                {{ $t("action.save") }}
              </el-button>
            </td>
            <td>
              <div class="d-flex align-items-center gap-2">
                <el-button
                  type="primary"
                  @click="showPaymentDetail(item, services)"
                  :disabled="isSubmitting"
                >
                  {{ $t("action.detail") }}
                </el-button>
                <el-button
                  v-if="!hideActive && !item.status"
                  type="danger"
                  @click="deletePaymentMethod(item)"
                  :disabled="isSubmitting"
                  plain
                >
                  {{ $t("action.delete") }}
                </el-button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <paymentServiceDetail ref="paymentDetailRef" @update="getServices" />
    <paymentServiceCreate
      ref="paymentCreateRef"
      @event-submit="onEventSubmitted"
    />
    <PaymentSortModal ref="paymentSortModalRef" @update="getServices" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch } from "vue";
import PaymentService from "../services/PaymentService";
import paymentServiceDetail from "../components/PaymentServiceDetail.vue";
import paymentServiceCreate from "../components/PaymentServiceCreate.vue";
import PaymentSortModal from "../components/modal/PaymentSortModal.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { ElNotification, ElMessageBox } from "element-plus";
import notification from "@/core/plugins/notification";

const store = useStore();
const tab = ref("Deposit");
const { t } = useI18n();
const services = ref(Array<any>());
const tempServices = ref(Array<any>());
const isLoading = ref(true);
const isSubmitting = ref(false);
const paymentDetailRef = ref<any>(null);
const paymentCreateRef = ref<any>(null);
const paymentSortModalRef = ref<any>(null);
const updateHistory = ref(Array<any>());
const platform = ref(Array<any>());
const hideActive = ref(true);
const highDollarInfo = ref({} as any);
const callbackTime = ref(null);
const criteria = ref({
  page: 1,
  size: 100,
  sortField: "Sequence",
  sortFlag: false,
});

const updateCallbackTime = async () => {
  isSubmitting.value = true;
  try {
    await PaymentService.updateCallbackTime({
      CallbackExpiredTimeInMinutes: callbackTime.value,
    });
    notification.success();
  } catch (error) {
    console.log(error);
    notification.danger();
  }
  isSubmitting.value = false;
};

const updateHighDollarConfigValue = async () => {
  isSubmitting.value = true;
  try {
    await PaymentService.updateHighDollarConfig(0, {
      value: highDollarInfo.value.value,
    });
    MsgPrompt.success("Save successfully");
  } catch (error) {
    console.log(error);
  }
  isSubmitting.value = false;
};

const getServices = async () => {
  isLoading.value = true;
  try {
    const [callBack, highDollar, auditBody, responseBody] = await Promise.all([
      PaymentService.getCallBackInfo().then((res) => {
        if (res) {
          callbackTime.value = res.value["callbackExpiredTimeInMinutes"];
        }
      }),
      PaymentService.getHighDollarLatestInfo().then((res) => {
        if (res) {
          highDollarInfo.value = {
            value: JSON.parse(res?.values.currentValues.Value).Value,
            staff: res?.user.displayName,
          };
        }
      }),
      PaymentService.getPaymentServicesUpdateBy({
        type: 23,
      }),
      tab.value == "Deposit"
        ? PaymentService.getDepositPaymentMethodsV2(criteria.value)
        : PaymentService.getWithdrawPaymentMethodsV2(criteria.value),
    ]);

    services.value = responseBody;
    services.value = services.value.map((item) => {
      return {
        ...item,
        isUpdated: false,
        group: item.group?.trim(),
        status: item.status === 10,
        updatedBy: auditBody.data.find(
          (x) =>
            x.rowId === item.id &&
            Object.keys(x.values.originalValues).length > 0
        ),
      };
    });

    const updatedBy = services.value
      .map((item) => item.updatedBy)
      .filter((updated) => updated !== undefined);

    platform.value = [
      ...new Set(services.value.map((service) => service.platform)),
    ];
    services.value = services.value.reduce((index, item) => {
      const cleanedGroup = (item.group ?? "")
        .replace(/（.*?）|\(.*?\)/g, "")
        .replace(/\s+/g, "")
        .trim();
      const group = item.isExLinkGlobal
        ? "ExLink Global"
        : cleanedGroup || "System Manual";

      if (index[group]) {
        index[group].push(item);
      } else {
        index[group] = [item];
      }
      return index;
    }, {});

    updateHistory.value = updatedBy.reduce((index, item) => {
      index[item.rowId] = {
        createdOn: formatDateTime(item.createdOn),
        userName: item.user.firstName + " " + item.user.lastName,
        previous: {
          [t("fields.activate")]: boolValue(item.values.originalValues?.status),

          [t("fields.initMinAmount")]: item.values.originalValues?.InitialValue,
          [t("fields.minAmount")]: item.values.originalValues?.MinValue,
          [t("fields.maxAmount")]: item.values.originalValues?.MaxValue,
          [t("fields.category")]: item.values.originalValues?.group?.trim(),
        },
        current: {
          [t("fields.activate")]: boolValue(item.values.currentValues?.status),

          [t("fields.initMinAmount")]: item.values.currentValues?.InitialValue,
          [t("fields.minAmount")]: item.values.currentValues?.MinValue,
          [t("fields.maxAmount")]: item.values.currentValues?.MaxValue,
          [t("fields.category")]: item.values.currentValues?.group?.trim(),
        },
      };

      return index;
    }, {});
    // 深拷贝保存完整数据，避免被 hideInactive 影响
    tempServices.value = JSON.parse(JSON.stringify(services.value));
    hideInactive();
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const changeTab = (_tab: any) => {
  tab.value = _tab;
  getServices();
};

onMounted(async () => {
  getServices();
});

watch(hideActive, (value) => {
  if (value) {
    services.value = Object.keys(services.value)
      .filter((category) => {
        return services.value[category].some((item) => {
          return item.status;
        });
      })
      .reduce((index, category) => {
        index[category] = services.value[category].filter((item) => {
          return item.status;
        });
        return index;
      }, {});
  } else {
    services.value = tempServices.value;
  }
  console.log("watch:services.value", services.value);
});

const hideInactive = () => {
  if (hideActive.value) {
    services.value = Object.keys(services.value)
      .filter((category) => {
        return services.value[category].some((item) => {
          return item.status;
        });
      })
      .reduce((index, category) => {
        index[category] = services.value[category].filter((item) => {
          return item.status;
        });
        return index;
      }, {});
  } else {
    services.value = tempServices.value;
  }
};

const showPaymentDetail = (item: any, category: any) => {
  paymentDetailRef.value?.show(item, category);
};

const deletePaymentMethod = async (item: any) => {
  try {
    await ElMessageBox.confirm(t("title.confirmDelete"), t("tip.warning"), {
      confirmButtonText: t("action.confirm"),
      cancelButtonText: t("action.cancel"),
      type: "warning",
    });
    isSubmitting.value = true;
    await PaymentService.deletePaymentMethodById(item.id);
    notification.success();
    getServices();
  } catch (error: any) {
    if (error !== "cancel") {
      console.log(error);
      notification.danger();
    }
  } finally {
    isSubmitting.value = false;
  }
};

const showPaymentCreate = (category: any) => {
  paymentCreateRef.value?.show(category, platform.value, tab.value);
};

const showSortModal = () => {
  paymentSortModalRef.value?.show(services.value);
};

const singleSubmit = async (item: any) => {
  isSubmitting.value = true;

  const payload = ref({} as any);
  payload.value = JSON.parse(JSON.stringify(item));
  if (payload.value.status) {
    payload.value.status = 10;
  } else {
    payload.value.status = 0;
  }

  try {
    await updateStatus(item.id, payload.value)
      .then(() => {
        ElNotification({
          title: payload.value.name,
          message: " updated successfully",
          type: "success",
        });
      })
      .catch(() => {
        ElNotification({
          title: payload.value.name,
          message: "Updated failed",
          type: "error",
        });
      });
  } catch (error) {
    console.log(error);
  }
  isSubmitting.value = false;

  // getServices();
};

const resetCache = async () => {
  isLoading.value = true;
  try {
    await PaymentService.PaymentMethodsResetCache();
    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      getServices();
    });
  } catch (error) {
    MsgPrompt.error(t("tip.submitError"));
  }
  isLoading.value = false;
};

const onEventSubmitted = () => {
  getServices();
};

const updateStatus = async (id: number, item: any) => {
  try {
    await PaymentService.putPaymentMethodByIdV2(id, {
      ...item,
    });
  } catch (error) {
    console.log(error);
  }
};

const boolValue = (value: number) => {
  if (value == 1) return "On";
  else if (value == 0) return "Off";
};

const formatDateTime = (isoString) => {
  let dateObj = new Date(isoString);
  let formattedDate = new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  }).format(dateObj);
  let formattedTime = new Intl.DateTimeFormat("en-US", {
    hour12: false,
    hour: "2-digit",
    minute: "2-digit",
  }).format(dateObj);
  return `${formattedDate} ${formattedTime}`;
};
</script>
<style scoped lang="scss">
.tbody {
  border-bottom: 1px solid #00000023;
}
</style>
