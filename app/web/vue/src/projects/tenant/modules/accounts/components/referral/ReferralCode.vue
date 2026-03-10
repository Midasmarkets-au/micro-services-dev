<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">{{ $t("fields.referralCode") }}</div>

      <div class="d-flex">
        <div class="ms-5 d-flex align-items-center">
          <!-- <div class="me-3">Wallet ID</div> -->
          <el-input
            class="w-150px"
            v-model="criteria.code"
            :placeholder="$t('fields.code')"
          >
          </el-input>

          <el-button class="ms-5" type="primary" @click="fetchData(1)" plain
            >{{ $t("action.search") }}
          </el-button>

          <el-button
            class="ms-3"
            type="warning"
            @click="(criteria.code = ''), fetchData(1)"
            plain
            >{{ $t("action.clear") }}
          </el-button>
        </div>
      </div>
    </div>
    <div class="card-body">
      <table class="table-tenant">
        <thead>
          <tr>
            <th>{{ $t("fields.referralCode") }}</th>
            <th>{{ $t("fields.referralCodeName") }}</th>
            <th>Party Id</th>
            <th>{{ $t("fields.accountId") }}</th>
            <th>{{ $t("fields.serviceType") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.code }}</td>
            <td>{{ item.name }}</td>
            <td>{{ item.partyId }}</td>
            <td>{{ item.accountId }}</td>
            <td>{{ AccountRoleTypes[item.serviceType] }}</td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td>
              <el-button
                type="primary"
                @click="showSchema(item.displaySummary)"
              >
                {{ $t("action.viewScheme") }} (SuperAdmin)
              </el-button>
              <el-button type="success" @click="showDetail(item)">{{
                $t("action.detail")
              }}</el-button>
              <el-button
                type="warning"
                v-if="item.displaySummary.isAutoCreatePaymentMethod == 1"
                @click="showPaymentMethodModal(item, 'deposit')"
                :loading="paymentLoading && currentReferralId === item.id"
              >
                {{ $t("action.defaultDepositPaymentChannel") }}
              </el-button>
              <el-button
                type="warning"
                v-if="item.displaySummary.isAutoCreatePaymentMethod == 1"
                @click="showPaymentMethodModal(item, 'withdrawal')"
                :loading="paymentLoading && currentReferralId === item.id"
              >
                {{ $t("action.defaultWithdrawalPaymentChannel") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
  </div>
  <el-dialog
    v-model="show"
    width="700"
    align-center
    style="max-height: 95vh; overflow: auto"
  >
    <div class="d-flex justify-content-center" v-if="infoLoading">
      <LoadingRing />
    </div>
    <div v-else>
      <div v-if="typeof dialogData != 'object'">{{ dialogData }}</div>
      <VueJsonView v-else :src="dialogData" theme="rjv-default" />
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="show = false">{{ $t("action.close") }}</el-button>
      </div>
    </template>
  </el-dialog>

  <el-dialog
    v-model="showPaymentModal"
    :title="
      curentPaymentSetting === 'deposit'
        ? $t('title.defaultDepositPaymentChannel')
        : $t('title.defaultWithdrawalPaymentChannel')
    "
    width="600"
    align-center
  >
    <div class="d-flex justify-content-center" v-if="paymentLoading">
      <LoadingRing />
    </div>
    <div v-else>
      <el-form label-width="150px">
        <el-form-item :label="$t('fields.paymentMethod')">
          <el-select
            v-model="selectedPaymentMethods"
            multiple
            :placeholder="$t('tip.paymentMethodRequired')"
            style="width: 100%"
          >
            <el-option
              v-for="method in paymentMethods"
              :key="method.id"
              :label="method.name"
              :value="method.id"
            />
          </el-select>
        </el-form-item>
      </el-form>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="showPaymentModal = false">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="savePaymentMethods" :loading="saving">
          {{ $t("action.save") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import AccountService from "../../services/AccountService";
import VueJsonView from "@matpool/vue-json-view";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const isLoading = inject<any>("isLoading");
const infoLoading = ref(false);
const show = ref(false);
const curentPaymentSetting = ref("deposit");
const dialogData = ref(<any>[]);
const data = ref(<any>[]);
const criteria = ref({
  code: "",
  page: 1,
  size: 20,
});

// Payment methods modal
const showPaymentModal = ref(false);
const paymentLoading = ref(false);
const saving = ref(false);
const paymentMethods = ref<any[]>([]);
const selectedPaymentMethods = ref<number[]>([]);
const currentReferralId = ref<number>(0);
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await AccountService.getReferralCode(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};
const fetchDetail = async (code: string) => {
  infoLoading.value = true;
  try {
    const res = await AccountService.getReferralCodeDetailByCode(code);
    dialogData.value = res;
  } catch (error) {
    console.log(error);
  } finally {
    infoLoading.value = false;
  }
};
const showSchema = (data: any) => {
  show.value = true;
  dialogData.value = data;
};
const showDetail = (item: any) => {
  show.value = true;
  fetchDetail(item.code);
  dialogData.value = item;
};
const pageChange = (page: number) => {
  fetchData(page);
};

const fetchPaymentMethods = async (type: string) => {
  paymentLoading.value = true;
  try {
    let res = [];
    if (type == "deposit") {
      res = await AccountService.getDepositPaymentMethods({
        page: 1,
        size: 100,
        sortField: "Sequence",
        sortFlag: false,
      });
    } else {
      res = await AccountService.getWithDrawPaymentMethods({
        page: 1,
        size: 100,
        sortField: "Sequence",
        sortFlag: false,
      });
    }
    // 只显示 status == 10 的支付方式
    res = res?.filter((item: any) => item.status === 10) || [];
    paymentMethods.value = Array.isArray(res) ? res : [];
    console.log("Active payment methods loaded:", paymentMethods.value.length);
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  } finally {
    paymentLoading.value = false;
  }
};

const showPaymentMethodModal = async (item: any, type: string) => {
  currentReferralId.value = item.id;
  selectedPaymentMethods.value =
    type == "deposit"
      ? item.defaultAutoCreatePaymentMethod
      : item.defaultAutoCreateWithdrawalPaymentMethod || [];
  // 先加载数据，再打开 Modal
  paymentLoading.value = true;
  await fetchPaymentMethods(type);
  showPaymentModal.value = true;
  curentPaymentSetting.value = type;
};

const savePaymentMethods = async () => {
  if (
    !selectedPaymentMethods.value ||
    selectedPaymentMethods.value.length === 0
  ) {
    MsgPrompt.error(t("tip.paymentMethodRequired"));
    return;
  }

  saving.value = true;
  try {
    await AccountService.updateReferralDefaultPaymentMethods(
      currentReferralId.value,
      selectedPaymentMethods.value,
      curentPaymentSetting.value
    );
    MsgPrompt.success(t("tip.updatePaymentMethodSuccess"));
    showPaymentModal.value = false;
    // Refresh data to show updated values
    fetchData(criteria.value.page);
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  } finally {
    saving.value = false;
  }
};

onMounted(() => {
  fetchData(1);
});
</script>
