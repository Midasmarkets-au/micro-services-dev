<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <div>{{ $t("tip.paymentExpireTime") }}</div>
        <el-input
          type="number"
          v-model="expireTime"
          style="width: 75px"
          class="mx-4"
        ></el-input>
        <el-button
          type="warning"
          @click="updateExpireTime"
          :disabled="isSubmitting"
          plain
        >
          {{ $t("action.update") }}
        </el-button>
      </div>
      <div class="card-toolbar">
        <!-- <el-button
          type="primary"
          @click="openCreateModal"
          :disabled="isSubmitting"
        >
          {{ $t("action.add") }}
        </el-button> -->
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant table-hover">
        <thead>
          <tr>
            <th>{{ $t("fields.onOff") }}</th>
            <th>ID</th>
            <th>{{ $t("fields.name") }}</th>
            <th>{{ $t("fields.type") }}</th>
            <th>{{ $t("fields.walletAddress") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
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
              <el-switch
                v-if="item.status != CryptoStatusTypes.InUse"
                v-model="item.status"
                size="large"
                style="
                  --el-switch-on-color: #13ce66;
                  --el-switch-off-color: #ff4949;
                "
                inline-prompt
                :active-value="CryptoStatusTypes.Idle"
                :inactive-value="CryptoStatusTypes.Inactive"
                active-text="ON"
                inactive-text="OFF"
                @change="updateStatus(item)"
                :loading="isSubmitting"
              ></el-switch>
            </td>
            <td>{{ item.id }}</td>
            <td>{{ item.name }}</td>
            <td>{{ item.type }}</td>
            <td>{{ item.address }}</td>
            <td>
              <el-tag size="large" :type="getStatusTag(item.status)">{{
                getStatusLabel(item.status)
              }}</el-tag>
            </td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" type="GMToneLiner" />
            </td>
            <td>
              <el-button
                type="primary"
                @click="view(item)"
                plain
                :disabled="isSubmitting"
              >
                {{ $t("action.view") }}
              </el-button>
              <el-button
                type="danger"
                v-show="item.status == CryptoStatusTypes.Inactive"
                @click="deleteCrypto(item)"
                plain
                :disabled="isSubmitting"
              >
                {{ $t("action.delete") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <CryptoTransaction ref="cryptoTransactionRef" />
  <CreateCryptoWallet
    ref="createCryptoWalletRef"
    @eventSubmit="onWalletCreated"
  />
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import PaymentService from "../services/PaymentService";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import CryptoTransaction from "../components/crypto/CryptoTransaction.vue";
import CreateCryptoWallet from "../components/modal/CreateCryptoWallet.vue";
import {
  CryptoStatusTypeOptions,
  CryptoStatusTypes,
} from "@/core/types/crypto/CryptoStatus";
import notification from "@/core/plugins/notification";
import { ElMessageBox } from "element-plus";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const isLoading = ref(false);
const isSubmitting = ref(false);
const cryptoTransactionRef = ref<any>(null);
const createCryptoWalletRef = ref<any>(null);
const accountSelected = ref(0);
const expireTime = ref(0);
const expireData = ref<any>([]);
const data = ref(<any>[]);
const criteria = ref<any>({
  page: 1,
  size: 30,
});

const updateStatus = async (item: any) => {
  isSubmitting.value = true;
  try {
    await PaymentService.updateCryptoWalletStatus(item.id, item.status);
    notification.success();
  } catch (error) {
    console.log(error);
    notification.danger();
  }
  isSubmitting.value = false;
};

const fecthExpireTime = async () => {
  try {
    const res = await PaymentService.queryCryptoExpireTime(
      "public",
      0,
      "CryptoSetting"
    );
    expireTime.value = res.value["payExpiredTimeInMinutes"];
    expireData.value = res;
  } catch (error) {
    console.log(error);
  }
};

const updateExpireTime = async () => {
  isSubmitting.value = true;
  try {
    await PaymentService.updateCryptoExpireTime("public", 0, "CryptoSetting", {
      key: expireData.value.key,
      name: expireData.value.name,
      dataFormat: expireData.value.dataFormat,
      value: { payExpiredTimeInMinutes: expireTime.value },
    });
    notification.success();
  } catch (error) {
    console.log(error);
    notification.danger();
  }
  isSubmitting.value = false;
};

const fecthData = async () => {
  isLoading.value = true;
  try {
    const res = await PaymentService.queryCryptoWallets(criteria.value);
    data.value = res;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const getStatusLabel = (value: number) => {
  const option = CryptoStatusTypeOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};

function getStatusTag(status: CryptoStatusTypes) {
  switch (status) {
    case CryptoStatusTypes.Idle:
      return "success";
    case CryptoStatusTypes.InUse:
      return "warning";
    case CryptoStatusTypes.Inactive:
      return "danger";
  }
}

const view = (item: any) => {
  cryptoTransactionRef.value.show(item);
};

const deleteCrypto = async (item: any) => {
  try {
    await ElMessageBox.confirm(t("title.confirmDelete"), t("tip.warning"), {
      confirmButtonText: t("action.confirm"),
      cancelButtonText: t("action.cancel"),
      type: "warning",
    });
    isSubmitting.value = true;
    await PaymentService.deleteCryptoWallet(item.id);
    notification.success();
    fecthData();
  } catch (error: any) {
    if (error !== "cancel") {
      console.log(error);
      notification.danger();
    }
  } finally {
    isSubmitting.value = false;
  }
};

const selectedAccount = (id: number) => {
  accountSelected.value = id;
};

const openCreateModal = () => {
  createCryptoWalletRef.value?.show();
};

const onWalletCreated = () => {
  fecthData();
};

onMounted(() => {
  fecthData();
  fecthExpireTime();
});
</script>
