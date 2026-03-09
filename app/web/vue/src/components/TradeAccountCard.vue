<template>
  <div
    v-click-outside="handleClickOutside"
    @mouseenter="onMouseEnter"
    @mouseleave="onMouseLeave"
    class="account-card rounded-3 overflow-hidden"
    :class="{
      clicked: boxClicked,
      hovered: hovered,
      demo: type === 'demo',
      real: type === 'account' || type === 'application',
    }"
  >
    <div
      class="top-box"
      :class="{
        'top-box-account': type === 'account' || type === 'application',
        'top-box-demo': type === 'demo',
      }"
    >
      <div class="d-flex flex-column gap-0">
        <div class="d-flex justify-content-between">
          <div class="d-flex align-self-start">
            <div
              v-if="type === 'account' || type === 'application'"
              class="fs-7 me-2 badge_real"
            >
              {{
                serviceMap[item?.tradeAccount?.serviceId]?.serverName ??
                serviceMap[item?.supplement?.serviceId]?.serverName
              }}
            </div>

            <div v-if="type === 'demo'" class="fs-7 me-2 badge_demo">
              {{ serviceMap[item.serviceId]?.serverName }}
            </div>

            <div
              v-if="type === 'account' && item.hasTradeAccount"
              class="fs-7 me-2 badge_mt"
              :style="{
                'background-color':
                  serviceMap[item?.tradeAccount?.serviceId]?.platformName ===
                  'MT4'
                    ? '#2BA580'
                    : serviceMap[item?.tradeAccount?.serviceId]
                        ?.platformName === 'MT5'
                    ? '#52C7DB'
                    : '#FFC107',
              }"
            >
              {{ serviceMap[item?.tradeAccount?.serviceId]?.platformName }}
            </div>
            <div
              v-if="
                type === 'account' &&
                item.hasTradeAccount &&
                item.fundType === 1
              "
              class="fs-7 me-2 badge_real"
            >
              {{ getWireTypeByTenant(item) }}
            </div>
            <div
              v-else-if="type === 'application'"
              class="badge-success fs-7 me-2 badge_mt"
              :style="{
                'background-color':
                  serviceMap[item?.supplement?.serviceId]?.platformName ===
                  'MT4'
                    ? '#2BA580'
                    : serviceMap[item?.supplement?.serviceId]?.platformName ===
                      'MT5'
                    ? '#52C7DB'
                    : '#FFC107',
              }"
            >
              {{ serviceMap[item?.supplement?.serviceId]?.platformName }}
            </div>

            <div
              v-else-if="type === 'demo'"
              class="badge-success fs-7 me-2 badge_mt"
              :style="{
                'background-color':
                  serviceMap[item.serviceId]?.platformName === 'MT4 Demo'
                    ? '#2BA580'
                    : serviceMap[item.serviceId]?.platformName === 'MT5 Demo'
                    ? '#52C7DB'
                    : '#FFC107',
              }"
            >
              {{ serviceMap[item.serviceId]?.platformName }}
            </div>

            <!-- <div v-else class="fs-7 me-2 badge_real"></div> -->
          </div>
          <span class="svg-icon svg-icon-2x">
            <template v-if="!disableSetting"
              ><div>
                <button
                  type="button"
                  :disabled="!(type === 'account' && item.hasTradeAccount)"
                  class="setting-btn btn btn-icon btn-active-white"
                  data-kt-menu-trigger="click"
                  data-kt-menu-attach="parent"
                  data-kt-menu-placement="bottom-end"
                  data-kt-menu-flip="bottom"
                  @click="showTradeAccountSettings"
                >
                  <span class="svg-icon svg-icon-2x" :class="{}">
                    <inline-svg src="/images/icons/general/setting.svg" />
                  </span>
                </button>

                <div
                  class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-semobold py-2 w-200px"
                  data-kt-menu="true"
                >
                  <div
                    class="menu-item px-2"
                    v-if="
                      item?.tradeAccount?.accountNumber &&
                      ($can('SuperAdmin') || wholesaleEnable)
                    "
                  >
                    <div class="menu-link">
                      <router-link
                        :to="
                          '/account/' +
                          item?.tradeAccount?.accountNumber +
                          '/apply-wholesale'
                        "
                      >
                        {{ $t("action.upgradeWholesale") }}
                      </router-link>
                    </div>
                  </div>

                  <div class="menu-item px-2">
                    <div
                      class="menu-link"
                      :class="{
                        'menu-disabled': passwordChangeActivity,
                      }"
                      @click="
                        !passwordChangeActivity
                          ? emits('onResetPassword')
                          : () => {}
                      "
                    >
                      {{ $t("action.changePassword") }}
                    </div>
                  </div>

                  <div class="menu-item px-2">
                    <div
                      class="menu-link"
                      :class="{
                        'menu-disabled': leverageChangeActivity,
                      }"
                      @click="
                        !leverageChangeActivity
                          ? emits('onChangeLeverage')
                          : () => {}
                      "
                    >
                      {{ $t("action.changeLeverage") }}
                    </div>
                  </div>

                  <div class="menu-item px-2">
                    <router-link
                      :to="'/account/' + item.tradeAccount?.accountNumber"
                      class="menu-link"
                    >
                      {{ $t("action.details") }}
                    </router-link>
                  </div>
                </div>
              </div>
            </template>
          </span>
        </div>
      </div>

      <div class="d-flex flex-column gap-0 pl-6" v-if="type !== 'ib'">
        <div class="d-flex gap-2 align-items-center">
          <div
            class="fw-semibold"
            :style="{
              color: type === 'application' ? '#0A46AA' : '#0A46AA',
              'font-size': '28px',
            }"
          >
            <BalanceShow
              v-if="type === 'account' && item.hasTradeAccount"
              :balance="item?.tradeAccount?.equityInCents"
              :currency-id="item?.tradeAccount?.currencyId"
            />
            <span class="" v-else-if="type === 'application'">
              {{ $t("status.pending") + "..." }}
            </span>
            <BalanceShow
              v-else-if="type === 'demo'"
              :balance="item.balanceInCents"
              :currency-id="item.currencyId"
              style="color: #4d4d4d"
            />
            <span v-else> *** </span>
          </div>
        </div>
        <span class="text-black d-block mt-2 mb-2">
          {{ $t("fields.equity") }}
        </span>
      </div>
    </div>
    <!---中间-->
    <div class="mid-box row g-0">
      <div class="col-4 mb-4 d-flex flex-column gap-1">
        <div>
          <CopyBox
            v-if="type === 'account' && item.hasTradeAccount"
            :val="item?.tradeAccount?.accountNumber + ''"
            :hasIcon="true"
          />

          <CopyBox
            v-else-if="type === 'demo'"
            :val="item.accountNumber + ''"
            :hasIcon="true"
          />

          <CopyBox
            v-else-if="type === 'ib'"
            :val="item.uid + ''"
            :hasIcon="true"
          />
          <span v-else class=""> {{ $t("status.pending") + "..." }} </span>
        </div>
        <span class="info-label">
          <template v-if="type === 'ib'">
            {{ $t("fields.uid") }}
          </template>
          <template v-else>
            {{ $t("fields.accountNo") }}
          </template>
        </span>
      </div>
      <div class="col-2 mb-4 d-flex flex-column gap-1">
        <div>
          <template
            v-if="
              (type === 'account' && item.hasTradeAccount) ||
              type === 'ib' ||
              type === 'demo'
            "
            >{{ getAccountTypeByTenant(item) }}
          </template>

          <template v-else-if="type === 'application'">
            {{ $t(`type.account.${item.supplement.accountType}`) }}
          </template>

          <template v-else> *** </template>
        </div>
        <span class="info-label">{{ $t("fields.type") }}</span>
      </div>
      <div class="col-2 mb-4 d-flex flex-column gap-1">
        <div>
          <template v-if="type === 'account' && item.hasTradeAccount">
            {{ item?.tradeAccount?.leverage + ":1" }}
          </template>
          <template v-else-if="type === 'application'">
            {{
              item?.supplement?.leverage === null
                ? "***"
                : item?.supplement?.leverage + ":1"
            }}
          </template>
          <template v-else-if="type === 'demo'">
            {{ item.leverage + ":1" }}
          </template>
          <template v-else> *** </template>
        </div>
        <span class="info-label">{{ $t("fields.leverage") }}</span>
      </div>
      <div
        v-if="type === 'demo'"
        class="col-4 mb-4 d-flex flex-column gap-1 align-items-end"
      >
        <div :class="isMobile ? 'text-wrap' : 'text-nowrap'">
          <TimeShow
            v-if="item.expireOn !== null"
            :date-iso-string="item.expireOn"
            format="D MMMM YYYY"
          />
        </div>
        <span class="info-label">{{ $t("fields.expireDate") }}</span>
      </div>
      <div v-else class="col-4 mb-4 d-flex flex-column gap-1">
        <div>
          <BalanceShow
            :balance="item.tradeAccount?.creditInCents"
            :currency-id="item.currencyId"
            style="color: #4d4d4d"
          />
        </div>
        <span class="info-label">{{ $t("fields.Credit") }}</span>
      </div>
    </div>
    <!-- <div class="separator"></div> -->
    <!---底部-->
    <div
      class="bottom-box d-flex align-items-center py-1 flex-grow-1"
      :class="
        showWebtrader ? 'justify-content-between' : 'justify-content-between'
      "
    >
      <div>
        <div class="account-currency">
          <span class="svg-icon svg-icon-1x" :class="{}">
            <inline-svg
              v-if="type === 'account' && item.hasTradeAccount"
              :src="
                '/images/currency/' + item?.tradeAccount?.currencyId + '.svg'
              "
              style="width: 2.5rem; height: 2.5rem"
            />

            <inline-svg
              v-else-if="type === 'application'"
              :src="'/images/currency/' + item?.supplement?.currencyId + '.svg'"
              style="width: 2.5rem; height: 2.5rem"
            />

            <inline-svg
              v-else-if="type === 'demo'"
              :src="'/images/currency/' + item.currencyId + '.svg'"
              style="width: 2.5rem; height: 2.5rem"
            />
          </span>
        </div>
      </div>
      <div class="justify-content-end">
        <router-link
          v-if="showWebtrader && (type === 'account' || type === 'demo')"
          class="btn btn-xs text-primary btn-bordered btn-radius mr-2"
          :class="{
            'btn-light-secondary':
              (type === 'account' && item.hasTradeAccount) || type === 'ib',
          }"
          :to="webTraderLink"
        >
          WebTrader
        </router-link>
        <button
          class="btn btn-xs text-primary btn-bordered btn-radius"
          :class="{
            'btn-light-secondary':
              (type === 'account' && item.hasTradeAccount) || type === 'ib',
          }"
          :disabled="!buttonHandler"
          @click="buttonHandler"
        >
          {{ buttonText ?? "Action" }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, nextTick, computed } from "vue";
import BalanceShow from "@/components/BalanceShow.vue";
import CopyBox from "@/components/CopyBox.vue";
import { reinitializeComponents } from "@/core/plugins/plugins";
import TimeShow from "./TimeShow.vue";
import AccountService from "@/projects/client/modules/accounts/services/AccountService";
import {
  ApplicationStatusType,
  ApplicationType,
} from "@/core/types/ApplicationInfos";
import { ServiceTypes } from "@/core/types/ServiceTypes";
import { useI18n } from "vue-i18n";
import { getVerificationTenancy, tenancies } from "@/core/types/TenantTypes";
import {
  JpAccountTypeSelection,
  JpAccountStandOrAlpSelection,
} from "@/core/types/AccountInfos";
import { isMobile } from "@/core/config/WindowConfig";
const { t } = useI18n();
const props = withDefaults(
  defineProps<{
    showWebtrader?: boolean;
    item: any;
    serviceMap?: any;
    type?: string;
    style?: string;
    buttonHandler?: any;
    buttonText?: string;
    disableSetting?: boolean;
    wholesaleEnable?: boolean;
  }>(),
  {
    type: "account",
    style: "yellow",
    disableSetting: false,
  }
);

const webTraderLink = computed(() => {
  if (
    props.item.serviceId == ServiceTypes.MetaTrader5 ||
    props.item.serviceId == ServiceTypes.MetaTrader5Demo
  ) {
    return `/webTrader5/${props.item.accountNumber}`;
  } else {
    return `/webTrader/${props.item.accountNumber}/${props.item.serviceId}`;
  }
});

// const isMobile = computed(() => window.innerWidth < );
const leverageChangeActivity = ref<any>(null);
const passwordChangeActivity = ref<any>(null);

const boxClicked = ref(false);
const hovered = ref(false);
const onMouseEnter = () => {
  setTimeout(() => {
    hovered.value = true;
  }, 100);
};

const onMouseLeave = () => {
  setTimeout(() => {
    hovered.value = false;
  }, 100);
};

const emits = defineEmits<{
  (e: "onResetPassword"): void;
  (
    e: "onTransferIn",
    accountNumber: number,
    uid: number,
    currencyId: number
  ): void;
  (e: "onChangeLeverage"): void;
}>();

const handleClickOutside = () => {
  if (!boxClicked.value) return;
  boxClicked.value = false;
};

// watch(hovered, (newValue, oldValue) => {
//   setTimeout(() => {
//     hovered.value = oldValue;
//   }, 1);
// });

const showTradeAccountSettings = async () => {
  boxClicked.value = true;
  await getAccountChangePasswordAndLeverageActivities();
};

const getAccountChangePasswordAndLeverageActivities = async () => {
  //
  const leverageRes = await AccountService.queryApplications({
    referenceId: props.item.uid,
    type: ApplicationType.TradeAccountChangeLeverage,
    status: ApplicationStatusType.AwaitingApproval,
  });
  if (leverageRes.data.length > 0) {
    leverageChangeActivity.value = leverageRes.data[0];
  }

  const passwordRes = await AccountService.queryApplications({
    referenceId: props.item.uid,
    type: ApplicationType.TradeAccountChangePassword,
    status: ApplicationStatusType.AwaitingApproval,
  });

  if (passwordRes.data.length > 0) {
    passwordChangeActivity.value = passwordRes.data[0];
  }
};

const getWireTypeByTenant = (item) => {
  if (getVerificationTenancy() === tenancies.jp) {
    return JpAccountTypeSelection[item.type];
  }
  return t("type.fundType." + item.fundType);
};

const getAccountTypeByTenant = (item) => {
  if (getVerificationTenancy() === tenancies.jp) {
    return JpAccountStandOrAlpSelection[item.type];
  }
  return t("type.account." + item.type);
};

onMounted(async () => {
  await nextTick();
  reinitializeComponents();
});
</script>

<style scoped lang="scss">
.account-card {
  position: relative;
  width: 100%;
  //box-shadow: 0 3px 8px rgba(96, 93, 93, 0.24);
  @media (max-width: 767px) {
    // width: 280px; // Change width to 100% in mobile view
  }
  background-color: #fff;
  position: relative;
  &.real {
    &::before {
      content: "";
      position: absolute;
      width: 16rem;
      height: 10.42rem;
      background: radial-gradient(
        circle,
        rgba(22, 132, 252, 0.3),
        rgba(186, 184, 253, 0)
      );
      filter: blur(50px);
      z-index: 0;
      pointer-events: none;
    }
  }
  &.demo {
    &::before {
      content: "";
      position: absolute;
      width: 16rem;
      height: 10.42rem;
      background: radial-gradient(
        circle,
        rgba(252, 22, 93, 0.2),
        rgba(253, 184, 184, 0)
      );
      filter: blur(70px);
      z-index: 0;
      pointer-events: none;
    }
  }

  .menu-link {
    &::after {
      content: none !important;
      display: none !important;
    }
  }

  .top-box {
    position: relative;
    padding: 0px 1.07rem;
    border-bottom: 1px dashed #adcbff;
  }

  .mid-box {
    display: flex;
    font-size: 12px;
    padding: 8px 1rem 0px 1rem;
    color: #000f32;
    font-weight: 600;
    .info-label {
      font-size: 12px;
      font-weight: bold;
      color: #717171;
      text-transform: uppercase;

      @media (max-width: 767px) {
        //font-size: 12px;
      }
    }

    @media (max-width: 767px) {
      //padding: 30px 20px 20px 20px;
      font-size: 16px;
    }
  }
}

.top-box-account {
  /* background: linear-gradient(to bottom, #edb589, #f6d04c); */
  // background: linear-gradient(
  //   162.84deg,
  //   rgba(239, 93, 168, 0.49) -68.02%,
  //   #ffd400 105.58%
  // );
}
.badge_demo {
  color: #868d98;
  background: #f2f4f7;
  border-bottom-left-radius: 8px;
  border-bottom-right-radius: 8px;
  min-width: 97px;
  padding-top: 2px;
  text-align: center;
}
.badge_real {
  color: #0a46aa;
  background: linear-gradient(95deg, #dce5fc 0%, #a9c2f5 100%);
  border-bottom-left-radius: 8px;
  border-bottom-right-radius: 8px;
  padding-left: 10px;
  padding-right: 10px;
  padding-top: 2px;
  text-align: center;
  white-space: nowrap;
}
.badge_mt {
  color: #82001d;
  background: linear-gradient(
    95deg,
    rgb(254, 242, 245) 0%,
    rgb(255, 202, 215) 100%
  );
  min-width: 88px;
  padding-top: 2px;
  border-bottom-left-radius: 8px;
  border-bottom-right-radius: 8px;
  text-align: center;
}
.top-box-demo {
  //background-color: #e0e2ec;
}

.account-currency {
  // position: absolute;
  // top: 100%;
  // left: 50%;
  // transform: translate(-50%, -50%);
  // width: 60px;
  // height: 60px;
  // border-radius: 50%;
  // /* background: rgba(253, 202, 31, 0.2); */

  // display: flex;
  // justify-content: center;
  // align-items: center;

  @media (max-width: 767px) {
    width: 55px;
    height: 55px;
  }
}

.bottom-box {
  //height: 50px;
  padding: 0 1rem;
  margin-bottom: 10px;
  @media (max-width: 767px) {
    //height: 40px;
  }
}

@media (min-width: 767px) {
  .hovered {
    box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.4);
    //top: -10px;
    //animation: bounce 0.2s;
  }

  .clicked {
    box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.4);
  }

  @keyframes bounce {
    0% {
      top: 0;
    }
    50% {
      top: -6px;
    }
    75% {
      top: -8px;
    }
    100% {
      top: -10px;
    }
  }
}
</style>
