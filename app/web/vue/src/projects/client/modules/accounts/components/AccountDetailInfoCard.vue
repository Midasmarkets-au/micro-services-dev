<template>
  <div v-if="isLoading">Loading</div>
  <!--begin::List Widget 1-->
  <div v-else class="card mb-5 mb-xl-8">
    <!--begin::Body-->
    <div class="card-body p-0">
      <!--begin::Header-->
      <div class="bg-danger px-9 pt-7 card-rounded h-275px w-100">
        <!--begin::Heading-->
        <div class="d-flex flex-stack">
          <h3 class="m-0 text-white fw-bold fs-3">
            {{
              ": " +
              $t("fields.accountNumber") +
              accountInfo?.tradeAccount?.accountNumber
            }}
          </h3>

          <div class="ms-1">
            <!--begin::Menu-->
            <button
              type="button"
              :class="`btn-active-color-danger`"
              class="btn btn-sm btn-icon btn-color-white btn-active-white border-0 me-n3"
              data-kt-menu-trigger="click"
              data-kt-menu-placement="bottom-end"
              data-kt-menu-flip="top-end"
            >
              <span class="svg-icon svg-icon-2">
                <inline-svg src="/images/icons/general/gen024.svg" />
              </span>
            </button>
            <!--end::Menu-->
          </div>
        </div>
        <!--end::Heading-->

        <!--begin::Balance-->
        <div class="d-flex text-center flex-column text-white pt-8">
          <span class="fw-semobold fs-7">{{ $t("title.yourBalance") }}</span>
          <span class="fw-bold fs-2x pt-1">
            <BalanceShow
              :balance="accountInfo.tradeAccount.equityInCents"
              :currency-id="accountInfo.tradeAccount.currencyId"
              :isShow="false"
            />
          </span>
          <div class="d-flex justify-content-center gap-3 text-white">
            <button
              class="btn text-light-danger"
              @click="openPasswordChangePanel"
            >
              {{ $t("action.resetPassword") }}
            </button>
            <button
              class="btn text-light-danger"
              @click="openLeverageChangePanel"
            >
              {{ $t("action.changeLeverage") }}
            </button>
            <button class="btn text-light-danger" @click="openTransferInPanel">
              {{ $t("action.transferIn") }}
            </button>
            <button class="btn text-light-danger" @click="openTransferOutPanel">
              {{ $t("action.transferOut") }}
            </button>
            <button class="btn text-light-danger" @click="openDepositPanel">
              {{ $t("action.deposit") }}
            </button>
          </div>
        </div>
        <!--end::Balance-->
      </div>
      <!--end::Header-->

      <!--begin::Items-->

      <div
        class="bg-body shadow-sm card-rounded mx-9 mb-9 px-6 py-9 position-relative z-index-1"
        style="margin-top: -70px"
      >
        <template v-for="(item, index) in items" :key="index">
          <!--begin::Item-->
          <div
            :class="[index !== items.length && 'mb-7']"
            class="d-flex align-items-center"
          >
            <!--begin::Symbol-->
            <div class="symbol symbol-45px w-40px me-5">
              <span class="symbol-label bg-lighten">
                <span class="svg-icon svg-icon-1">
                  <inline-svg :src="item.icon" />
                </span>
              </span>
            </div>
            <!--end::Symbol-->

            <!--begin::Description-->
            <div class="d-flex align-items-center flex-wrap w-100">
              <!--begin::Title-->
              <div class="mb-1 pe-3 flex-grow-1">
                <a
                  href="#"
                  class="fs-5 text-gray-800 text-hover-primary fw-bold"
                  >{{ item.title }}</a
                >
                <div class="text-gray-400 fw-semobold fs-7">
                  {{ item.description }}
                </div>
              </div>
              <!--end::Title-->

              <!--begin::Label-->
              <div class="d-flex align-items-center">
                <div class="fw-bold fs-5 text-gray-800 pe-1">
                  {{ item.stats }}
                </div>

                <span
                  v-if="item.arrow === 'up'"
                  class="svg-icon svg-icon-5 svg-icon-success ms-1"
                >
                  <inline-svg src="/images/icons/arrows/arr066.svg" />
                </span>
                <span
                  v-else-if="item.arrow === 'down'"
                  class="svg-icon svg-icon-5 svg-icon-danger ms-1"
                >
                  <inline-svg src="/images/icons/arrows/arr065.svg" />
                </span>
              </div>
              <!--end::Label-->
            </div>
            <!--end::Description-->
          </div>
          <!--end::Item-->
        </template>
      </div>
      <!--end::Items-->
    </div>
    <!--end::Body-->
  </div>
  <!--end::List Widget 1-->
  <PasswordChangeForm ref="passwordChangeFormRef" />
  <LeverageChangeForm ref="leverageChangeFormRef" />
  <TransferInForm ref="transferInFormRef" />
  <TransferOutForm ref="transferOutFormRef" />
  <CreateDepositModal ref="depositFormRef" />
</template>

<script setup lang="ts">
import { onMounted, ref, watch, computed } from "vue";
import AccountService from "../services/AccountService";
import i18n from "@/core/plugins/i18n";
import PasswordChangeForm from "./modal/ResetPasswordForm.vue";
import LeverageChangeForm from "./modal/ChangeLeverageForm.vue";
import TransferInForm from "./modal/TransferInForm.vue";
import TransferOutForm from "./modal/TransferOutForm.vue";
import CreateDepositModal from "../../wallet/components/modal/CreateDepositModalTemp.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import GlobalService from "@/projects/client/services/ClientGlobalService";
const { t } = i18n.global;

const props = defineProps<{
  currentAccount: any;
}>();
const accountInfo = ref<any>({});
const serviceMap = ref<any>([]);
const passwordChangeFormRef = ref<InstanceType<typeof PasswordChangeForm>>();
const leverageChangeFormRef = ref<InstanceType<typeof LeverageChangeForm>>();
const transferInFormRef = ref<InstanceType<typeof TransferInForm>>();
const transferOutFormRef = ref<InstanceType<typeof TransferOutForm>>();
const depositFormRef = ref<InstanceType<typeof CreateDepositModal>>();
const isLoading = ref(true);

const items = computed(() => [
  {
    icon: "/images/icons/maps/map004.svg",
    title: "Name",
    description: "Description",
    stats: accountInfo.value.name,
    arrow: "up",
  },
  {
    icon: "/images/icons/general/gen024.svg",
    title: "Role",
    description: "Description",
    stats:
      accountInfo.value.role &&
      t(`type.accountRole.${accountInfo.value?.role}`),
    arrow: "down",
  },
  {
    icon: "/images/icons/electronics/elc005.svg",
    title: "Server",
    description: "Description",
    stats:
      serviceMap.value[accountInfo.value?.tradeAccount?.serviceId]?.serverName,
    arrow: "up",
  },
  {
    icon: "/images/icons/general/gen005.svg",
    title: "Created On",
    description: "Description",
    stats: accountInfo.value.createdOn,
    arrow: "down",
  },
]);

const getAccountInfo = async () => {
  const { data } = await AccountService.queryAccounts({
    uid: props.currentAccount.uid,
    hasTradeAccount: true,
  });
  if (data.length === 0) throw new Error("No Account Found");
  accountInfo.value = data[0];
};

const openPasswordChangePanel = () => {
  passwordChangeFormRef.value?.show(accountInfo.value.tradeAccount);
};

const openLeverageChangePanel = () => {
  leverageChangeFormRef.value?.show(accountInfo.value.tradeAccount);
};

const openTransferInPanel = () => {
  transferInFormRef.value?.show(
    accountInfo.value.tradefields.accountNumber,
    props.currentAccount.uid,
    accountInfo.value.tradeAccount.currencyId
  );
};

const openTransferOutPanel = () => {
  transferOutFormRef.value?.show(
    accountInfo.value.tradefields.accountNumber,
    props.currentAccount.uid,
    accountInfo.value.tradeAccount.currencyId,
    accountInfo.value.tradeAccount.balance
  );
};

const openDepositPanel = () => {
  depositFormRef.value?.show(
    props.currentAccount.uid,
    accountInfo.value.tradeAccount.currencyId
  );
};

onMounted(async () => {
  serviceMap.value = await GlobalService.getServiceMap();
  await getAccountInfo();
  isLoading.value = false;
});

watch(
  () => props.currentAccount.uid,
  async () => {
    await getAccountInfo();
  }
);
</script>
