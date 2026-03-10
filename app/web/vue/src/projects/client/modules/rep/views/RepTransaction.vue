<template>
  <div>
    <div v-if="!repAccount">{{ $t("action.noRepAccount") }}</div>
    <div v-else>
      <div class="sub-menu d-flex">
        <router-link to="/rep" class="sub-menu-item">{{
          $t("title.customer")
        }}</router-link>
        <router-link to="/rep/trade" class="sub-menu-item">{{
          $t("title.trade")
        }}</router-link>
        <router-link to="/rep/transaction" class="sub-menu-item active">{{
          $t("title.transfer")
        }}</router-link>
        <router-link to="/rep/deposit" class="sub-menu-item">{{
          $t("title.deposit")
        }}</router-link>
        <router-link to="/rep/withdrawal" class="sub-menu-item">{{
          $t("title.withdrawal")
        }}</router-link>
        <!-- <router-link to="/rep/lead" class="sub-menu-item">{{
          $t("title.repLeadSystem")
        }}</router-link> -->
      </div>
    </div>

    <div class="card round-bl-br px-5">
      <RepTransactionFilter
        v-if="!isMobile"
        ref="tradeFilterRef"
        :trigger="'button'"
        :filter-options="filterOptions"
        :service-handler="getFetchDataFunc"
        :default-criteria="defaultCriteria"
      />
    </div>
    <div class="card mt-2 round-tl-tr">
      <div class="card-header">
        <div class="card-title">
          <h2 class="fw-semibold" v-if="true">
            {{
              $t("action.showing") +
              " " +
              (tradeFilterRef?.criteria.total ?? "0") +
              " " +
              $t("title.results")
            }}
          </h2>
        </div>
        <div class="card-toolbar">
          <button
            v-if="false && tradeFilterRef?.data.length !== 0"
            class="border-0 bg-transparent d-flex align-items-center"
          >
            <span class="me-2" style="color: #4196f0"> </span>
            <span class="svg-icon svg-icon-3">
              <inline-svg src="/images/icons/general/download.svg" />
            </span>
          </button>
        </div>
      </div>
      <div class="card-body pt-0 overflow-auto" style="white-space: nowrap">
        <table
          class="table align-middle table-row-bordered gy-5"
          id="kt_ecommerce_rep_table"
        >
          <thead>
            <tr class="text-start gs-0">
              <th>{{ $t("fields.user") }}</th>
              <th>{{ $t("fields.email") }}</th>
              <th>{{ $t("fields.currency") }}</th>
              <th class="text-center">{{ $t("fields.sourceAccount") }}</th>
              <th class="text-center">{{ $t("fields.targetAccount") }}</th>
              <th>{{ $t("fields.amount") }}</th>
              <th>{{ $t("fields.status") }}</th>
              <th>{{ $t("fields.time") }}</th>
            </tr>
          </thead>
          <tbody v-if="tradeFilterRef?.isLoading">
            <LoadingRing />
          </tbody>
          <tbody
            v-else-if="
              !tradeFilterRef?.isLoading && tradeFilterRef?.data.length == 0
            "
          >
            <NoDataBox />
          </tbody>
          <tbody v-else>
            <tr
              class="text-start"
              v-for="(item, index) in tradeFilterRef?.getData()"
              :key="index"
            >
              <td>
                <div class="d-md-flex align-items-center">
                  <UserAvatar
                    :avatar="item.user.avatar"
                    :name="item.user.displayName"
                    size="40px"
                    class="me-3"
                    side="client"
                    rounded
                  />
                  <span>
                    {{
                      item.user.nativeName ||
                      item.user.displayName ||
                      `${item.user.firstName} ${item.user.lastNameName}`
                    }}
                  </span>
                </div>
              </td>

              <td class="text-start">
                {{ item.user.email }}
              </td>

              <td class="text-start">
                {{ $t(`type.currency.${item.currencyId}`) }}
              </td>

              <td class="text-center">
                <div class="d-flex flex-column align-items-center">
                  <span class="text-gray-800">
                    No.
                    <span class="">
                      {{ item.targetAccount.accountNumber }} ({{
                        $t(`type.currency.${item.targetAccount.currencyId}`)
                      }})</span
                    >
                  </span>

                  <span class="fs-7">
                    Group:
                    <span class="">
                      {{ item.targetAccount.group || "***" }}</span
                    >
                  </span>
                </div>
              </td>

              <td class="text-center">
                <div class="d-flex flex-column align-items-center">
                  <span class="text-gray-800">
                    No.
                    <span class="">
                      {{ item.sourceAccount.accountNumber }} ({{
                        $t(`type.currency.${item.sourceAccount.currencyId}`)
                      }})</span
                    >
                  </span>

                  <span class="fs-7">
                    Group:
                    <span class="">
                      {{ item.sourceAccount.group || "***" }}</span
                    >
                  </span>
                </div>
              </td>

              <td class="text-start">
                <BalanceShow
                  :balance="item.amount"
                  :currencyId="item.currencyId"
                />
              </td>
              <td class="text-start">
                <span
                  class="badge"
                  :class="`badge-${
                    {
                      [TransactionStateType.TransferCreated]: 'primary',
                      [TransactionStateType.TransferCompleted]: 'success',
                      [TransactionStateType.TransferRejected]: 'danger',
                      [TransactionStateType.TransferApproved]: 'warning',
                    }[item.stateId] ?? 'info'
                  }`"
                  >{{
                    $t(`type.transactionState.${item.stateId}`).replace(
                      /^transfer\s+/i,
                      ""
                    )
                  }}</span
                >
              </td>

              <td class="text-start">
                <TimeShow type="inFields" :date-iso-string="item.createdOn" />
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter
          @page-change="tradeFilterRef?.fetchData"
          :criteria="tradeFilterRef?.criteria"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch, computed, nextTick } from "vue";
import { useStore } from "@/store";
import RepService from "../services/RepService";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import TableFooter from "@/components/TableFooter.vue";
import TimeShow from "@/components/TimeShow.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import { useRoute, useRouter } from "vue-router";
import { isMobile } from "@/core/config/WindowConfig";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import UserAvatar from "@/components/UserAvatar.vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import RepTransactionFilter from "@/projects/client/modules/rep/components/RepTransactionFilter.vue";
import { TransactionStateType } from "@/core/types/StateInfos";

const store = useStore();
const route = useRoute();
const router = useRouter();
const isLoading = ref(true);
const repAccount = computed(() => store.state.RepModule.repAccount);
const tradeFilterRef = ref<InstanceType<typeof TradeFilter>>();
const filterOptions = ["size", "period", "accountNumber"];

const initialCriteria = ref<any>({
  page: 1,
  size: 10,
  isClosed: false,
  accountNumber: route.query?.accountNumber as string,
  // openedFrom: moment().startOf("day").toISOString(),
  // openedTo: moment().endOf("day").toISOString(),
});
const defaultCriteria = ref<any>(initialCriteria.value);

watch(repAccount, async () => {
  defaultCriteria.value = initialCriteria.value;
  await parseRoute();
  await nextTick();
  if (repAccount.value) {
    tradeFilterRef.value?.initCriteria();
    tradeFilterRef.value?.fetchData(1);
  }
});

const getFetchDataFunc = (criteria?: any) =>
  RepService.queryTransactionReportsOfRep(criteria);

const parseRoute = async () => {
  if (!route.query?.accountNumber) return;
  defaultCriteria.value.accountNumber = route.query.accountNumber as string;
  setTimeout(() => router.push({ query: {} }), 3000);
  await nextTick();
};

onMounted(async () => {
  isLoading.value = true;
  await parseRoute();
  isLoading.value = false;
});
</script>

<style scoped>
.card-body,
.sub-menu {
  overflow-x: auto;
  width: 100%;
  white-space: nowrap;
}
@media (max-width: 768px) {
  .table {
    font-size: 12px !important;
  }
}
</style>
