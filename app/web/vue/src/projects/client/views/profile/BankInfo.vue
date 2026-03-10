<template>
  <div class="mb-10" :class="isMobile ? 'px-3' : ''">
    <div>
      <CenterMenu activeMenuItem="bankInfo" />
    </div>
    <div class="card round-bl-br">
      <div class="card-header">
        <div class="card-title">{{ $t("title.linkedAccounts") }}</div>
        <div class="card-toolbar">
          <button
            @click="showCreateBankInfo()"
            class="btn btn-sm me-3 btn-light-primary btn-bordered mt-2"
          >
            <div class="d-flex align-items-center">
              <i class="fa-regular fa-plus"></i>
              <span>{{ $t("action.addNewAccount") }}</span>
            </div>
          </button>
          <button
            v-if="!hasUSDT"
            @click="showCreateUSDTForm()"
            class="btn btn-sm btn-light-primary btn-bordered mt-2"
          >
            <div class="d-flex align-items-center">
              <i class="fa-regular fa-plus"></i>
              <span>{{ $t("title.usdtWalletAddress") }}</span>
            </div>
          </button>
        </div>
      </div>
    </div>
    <div class="card mt-2 round-tl-tr">
      <div class="card-body" style="">
        <div v-if="isLoading" class="d-flex justify-content-center">
          <LoadingRing />
        </div>
        <div
          v-else-if="!isLoading && Object.keys(paymentMethods).length === 0"
          class="d-flex justify-content-center"
        >
          <NoDataBox />
        </div>
        <div v-else>
          <div>
            <div class="row">
              <div
                class="col-lg-4 mb-5"
                v-for="(item, index) in paymentMethods"
                :key="index"
              >
                <div class="bank-card">
                  <div class="row">
                    <div
                      class="col-2"
                      v-if="item.paymentPlatform == paymentPlateformList.USDT"
                    >
                      <img
                        :src="'/images/wallet/TRC20.png'"
                        :style="{
                          width: isMobile ? '30px' : '50px',
                          'border-radius': '100%',
                        }"
                      />
                    </div>
                    <div class="col-2" v-else>
                      <img
                        v-if="
                          availableCountries.includes(item.info.bankCountry)
                        "
                        :src="'/images/flags/' + item.info.bankCountry + '.svg'"
                        :style="{
                          width: isMobile ? '30px' : '50px',
                          'border-radius': '100%',
                        }"
                      />
                      <img
                        v-else
                        src="/images/wallet/BANK.png"
                        alt=""
                        :style="{
                          width: isMobile ? '30px' : '50px',
                          'border-radius': '100%',
                        }"
                      />
                    </div>
                    <div class="col-10">
                      <div class="d-flex justify-content-between">
                        <div>
                          <div>
                            <h3 class="text-primary font-medium fs-5">
                              {{ item.info.name }}
                              <span v-if="item.info.bankName"
                                >- {{ item.info.bankName }}</span
                              >
                              <span v-if="item.info.branchName"
                                >({{ item.info.branchName }})</span
                              >
                            </h3>
                          </div>
                          <div class="d-flex flex-column gap-2 mt-5">
                            <div class="bankinfo text-primary font-medium fs-5">
                              {{ item.info.holder }}
                            </div>
                            <div
                              class="bankinfo"
                              style="color: #3a3e44; font-size: 14px"
                            >
                              {{ item.info.accountNo }}
                            </div>
                          </div>
                        </div>
                        <!-- <div>
                          <el-icon
                            :size="20"
                            class="me-3 cursor-pointer mb-3"
                            @click="editBankInfo(item)"
                            ><Edit
                          /></el-icon>
                          <el-icon
                            :size="20"
                            @click="openConfirmBoxPanel(item.id)"
                            class="cursor-pointer"
                            ><CloseBold
                          /></el-icon>
                        </div> -->
                      </div>
                    </div>
                  </div>
                  <div class="row">
                    <div class="col-12 d-flex justify-content-end gap-5">
                      <button
                        @click="openConfirmBoxPanel(item.id)"
                        class="btn btn-light-primary btn-sm btn-radius btn-bordered"
                      >
                        {{ $t("action.cancel") }}
                      </button>
                      <button
                        @click="editBankInfo(item)"
                        class="btn btn-light-primary btn-sm btn-radius btn-bordered"
                      >
                        {{ $t("action.edit") }}
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <CreatePaymentAccountForm
      ref="CreatePaymentMethodFormRef"
      :paymentPlateformList="paymentPlateformList"
      @fetchData="fetchData"
    />
  </div>
  <!--begin::Basic info-->
</template>

<script lang="ts" setup>
import { onMounted, ref, inject } from "vue";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import CreatePaymentAccountForm from "./UserPaymentAccountForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { isMobile } from "@/core/config/WindowConfig";
import i18n from "@/core/plugins/i18n";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import CenterMenu from "./components/CenterMenu.vue";
import { Edit, CloseBold } from "@element-plus/icons-vue";
const paymentPlateformList = ref({
  Bank: 100,
  PayPal: 230,
  USDT: 240,
});
const openConfirmBoxModel = inject(
  ClientGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);

const { t } = i18n.global;
const selectedId = ref(-1);
const paymentMethods = ref([] as any);
const hasUSDT = ref(false);
const isLoading = ref(false);
const CreatePaymentMethodFormRef =
  ref<InstanceType<typeof CreatePaymentAccountForm>>();

onMounted(async () => {
  fetchData();
});

const fetchData = async () => {
  isLoading.value = true;

  try {
    await GlobalService.getUserPaymentInfo().then((res) => {
      paymentMethods.value = res.data;
      hasUSDT.value = paymentMethods.value.some(
        (item: any) => item.paymentPlatform == paymentPlateformList.value.USDT
      );
    });
  } catch (error) {
    console.log("error", error);
  }

  isLoading.value = false;
};

const openConfirmBoxPanel = (_id: number) => {
  selectedId.value = _id;
  openConfirmBoxModel?.(deleteBankInfoHandler);
};

const showCreateBankInfo = () => {
  CreatePaymentMethodFormRef.value?.show();
};
const showCreateUSDTForm = () => {
  CreatePaymentMethodFormRef.value?.showUSDT();
};
const editBankInfo = (item: any) => {
  CreatePaymentMethodFormRef.value?.show(item);
};

const deleteBankInfoHandler = async () => {
  try {
    await GlobalService.deleteUserPaymentInfo(selectedId.value);
    MsgPrompt.success(t("tip.paymentAccountCancelSuccess")).then(() => {
      fetchData();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const availableCountries = ref([
  "au",
  "cn",
  "tw",
  "vn",
  "th",
  "jp",
  "mn",
  "id",
  "my",
  "us",
]);
</script>

<style lang="scss" scoped>
.sub-menu {
  width: 100%;
  white-space: nowrap;
}
.bank-card {
  border-radius: 20px;
  padding: 20px 20px;
  background-color: #fff;
}

.bankinfo {
  color: #4d4d4d;
  font-size: 16px;
  font-family: Lato;
  font-weight: 400;
  line-height: 24px;
}
</style>
