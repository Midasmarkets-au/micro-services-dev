<template>
  <div>
    <div v-if="!agentAccount">{{ $t("action.noIbAccount") }}</div>
    <div v-else>
      <IBLayout activeMenuItem="withdrawal">
        <div
          class="card mb-2 pt-md-0 round-bl-br"
          :class="isMobile ? 'p-2' : 'pl-3 pt-2'"
        >
          <TradeFilter
            ref="filterRef"
            :trigger="'button'"
            :filter-options="filterOptions"
            :service-handler="getFetchDataFunc"
            :default-criteria="defaultCriteria"
            type="withdraw"
            class="ms-2"
          />
        </div>
        <div v-if="isMobile">
          <WithdrawalMobile />
          <TableFooter
            @page-change="filterRef?.fetchData"
            :criteria="filterRef?.criteria"
          />
        </div>
        <div v-else class="card round-tl-tr flex-1">
          <div class="card-header">
            <div class="card-title-noicon">
              <h2 class="font-medium fs-2">
                {{
                  $t("action.showing") +
                  " " +
                  (filterRef?.criteria.total ?? "0") +
                  " " +
                  $t("title.results")
                }}
              </h2>

              <div class="d-flex ms-5 fs-5 text-black">
                <div class="me-2">{{ $t("title.total") }}:</div>

                <BalanceShow
                  :balance="filterRef?.criteria.totalAmount"
                  :currency="filterRef?.data[0]?.currencyId"
                />
              </div>
            </div>
            <div class="card-toolbar">
              <el-switch
                class="me-50px"
                v-model="isSelectingClient"
                @change="onIsSelectingClientChange"
                width="65"
                size="large"
                inline-prompt
                style="
                  --el-switch-on-color: #0a46aa;
                  --el-switch-off-color: #000f32;
                "
                :active-text="$t('fields.client')"
                :inactive-text="$t('fields.ib')"
              />
            </div>
          </div>

          <div class="card-body pt-0 overflow-auto" style="white-space: nowrap">
            <table
              class="table align-middle table-row-bordered gy-3"
              id="table_accounts_requests"
            >
              <thead>
                <tr class="text-start gs-0">
                  <th>{{ $t("fields.user") }}</th>
                  <th>{{ $t("fields.email") }}</th>
                  <th>{{ $t("fields.currency") }}</th>
                  <th class="text-center" v-if="isSelectingClient">
                    {{ $t("fields.sourceAccount") }}
                  </th>
                  <th>{{ $t("fields.exchangeRate") }}</th>
                  <th>{{ $t("fields.amount") }}</th>
                  <th>{{ $t("fields.status") }}</th>
                  <th>{{ $t("fields.createdOn") }}</th>
                </tr>
              </thead>
              <tbody v-if="filterRef?.isLoading">
                <LoadingRing />
              </tbody>
              <tbody
                v-else-if="!filterRef?.isLoading && filterRef?.data.length == 0"
              >
                <NoDataBox />
              </tbody>

              <tbody v-else>
                <tr v-for="(item, index) in filterRef?.getData()" :key="index">
                  <td>
                    <div class="d-md-flex align-items-center">
                      <UserAvatar
                        :avatar="item.user.avatar"
                        :name="item.user.displayName"
                        size="54px"
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

                  <td class="text-start">{{ item.user.email }}</td>

                  <td class="text-start">
                    {{ $t(`type.currency.${item.currencyId}`) }}
                  </td>

                  <td class="text-center" v-if="isSelectingClient">
                    <div class="d-flex flex-column align-items-center">
                      <span class="text-gray-800">
                        No.
                        <span class="">
                          {{ item.source.displayNumber }} ({{
                            $t(`type.currency.${item.source.currencyId}`)
                          }})</span
                        >
                      </span>

                      <span class="fs-7">
                        Group:
                        <span class="">
                          {{ item.source.agentGroupName || "***" }}</span
                        >
                      </span>
                    </div>
                  </td>
                  <td class="text-center">{{ item.exchangeRate }}</td>

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
                          [TransactionStateType.WithdrawalCreated]: 'primary',
                          [TransactionStateType.WithdrawalCompleted]: 'success',
                          [TransactionStateType.WithdrawalTenantRejected]:
                            'danger',
                          [TransactionStateType.WithdrawalTenantApproved]:
                            'warning',
                        }[item.stateId] ?? 'info'
                      }`"
                      >{{
                        $t(`type.transactionState.${item.stateId}`).replace(
                          /^withdrawal\s+/i,
                          ""
                        )
                      }}</span
                    >
                  </td>

                  <td class="text-start">
                    <TimeShow
                      type="inFields"
                      :date-iso-string="item.createdOn"
                    />
                  </td>
                </tr>
              </tbody>
            </table>

            <TableFooter
              @page-change="filterRef?.fetchData"
              :criteria="filterRef?.criteria"
            />
          </div>
        </div>
      </IBLayout>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { computed, ref, provide } from "vue";
import IbService from "@/projects/client/modules/ib/services/IbService";
import TableFooter from "@/components/TableFooter.vue";
import UserAvatar from "@/components/UserAvatar.vue";
import TimeShow from "@/components/TimeShow.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import { TransactionStateType } from "@/core/types/StateInfos";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import IBLayout from "../components/IBLayout.vue";
//import headerMenu from "../components/menu/headerMenu.vue";
import { isMobile } from "@/core/config/WindowConfig";
import WithdrawalMobile from "../components/withdrawal/WithdrawalMobile.vue";

const store = useStore();
const agentAccount = computed(() => store.state.AgentModule.agentAccount);
const filterRef = ref<any>();
const filterOptions = ["period", "withdrawalState", "accountNumber"];

const isSelectingClient = ref(true);
const onIsSelectingClientChange = () => {
  //
  filterRef.value.criteria.role = isSelectingClient.value
    ? AccountRoleTypes.Client
    : AccountRoleTypes.IB;
  filterRef.value?.fetchData(1);
};

provide("filterRef", filterRef);
provide("isSelectingClient", isSelectingClient);
provide("onIsSelectingClientChange", onIsSelectingClientChange);

const initialCriteria = ref<any>({
  page: 1,
  size: 25,
  isClosed: false,
});
const defaultCriteria = ref<any>(initialCriteria.value);
const getFetchDataFunc = (criteria?: any) =>
  IbService.queryWithdrawal(criteria);
</script>

<style scoped></style>
