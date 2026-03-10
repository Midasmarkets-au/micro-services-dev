<template>
  <div>
    <div class="d-flex gap-4">
      <div>
        <div class="borders">
          <div class="head">Party ID</div>
          <div class="content">{{ data?.partyId }}</div>
        </div>
      </div>
      <div>
        <div class="borders">
          <div class="head">UID</div>
          <div class="content">{{ data?.uid }}</div>
        </div>
      </div>
      <div>
        <div class="borders">
          <div class="head">Account ID</div>
          <div class="content">{{ data?.id }}</div>
        </div>
      </div>
      <div v-if="data.accountNumber != 0">
        <div class="borders">
          <div class="head">{{ $t("fields.accountNumber") }}</div>
          <div class="content">{{ data?.accountNumber }}</div>
        </div>
      </div>
    </div>
    <el-divider />

    <!-- Basic Info -->
    <div class="px-2">
      <p class="title">{{ $t("fields.basicInfo") }}:</p>
      <div class="d-flex gap-4">
        <div class="borders">
          <div class="head">{{ $t("fields.userName") }}</div>
          <div class="content">{{ data?.name }}</div>
        </div>
        <div class="borders">
          <div class="head">{{ $t("fields.site") }}</div>
          <div class="content">
            {{ $t(`type.siteType.${data?.siteId}`) }}
          </div>
        </div>
        <div class="borders">
          <div class="head">{{ $t("fields.accountType") }}</div>
          <div class="content">
            {{ $t(`type.account.${data?.type}`) }}
          </div>
        </div>
        <div class="borders">
          <div class="head">{{ $t("fields.accountRole") }}</div>
          <div class="content">
            {{ $t(`type.accountRole.${data?.role}`) }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.group") }}</div>
          <div class="content">
            {{ data?.group }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.createdOn") }}</div>
          <div class="content">
            <TimeShow
              v-if="data.createdOn != null"
              :date-iso-string="data?.createdOn"
              format="YYYY-MM-DD"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Trading Account Info -->
    <el-divider v-if="data.hasTradeAccount == true" />
    <div v-if="data.hasTradeAccount == true" class="mt-6 px-2">
      <p class="title">{{ $t("fields.tradingAccountInfo") }}:</p>
      <div class="d-flex gap-4">
        <div class="borders">
          <div class="head">{{ $t("fields.accountNumber") }}</div>
          <div class="content">
            {{ data?.accountNumber }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.currency") }}</div>
          <div class="content">
            {{ t(`type.currency.${data.tradeAccount?.currencyId}`) }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.platform") }}</div>
          <div class="content">
            {{ ServiceTypes[data.tradeAccount?.serviceId] }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.group") }}</div>
          <div class="content">
            {{ data.tradeAccount.tradeAccountStatus?.group }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.lastLogin") }}</div>
          <div class="content">
            <TimeShow
              v-if="data.tradeAccount.tradeAccountStatus?.lastLoginOn != null"
              :date-iso-string="
                data.tradeAccount.tradeAccountStatus?.lastLoginOn
              "
              format="YYYY-MM-DD"
            />
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.createdOn") }}</div>
          <div class="content">
            <TimeShow
              v-if="data.tradeAccount?.createdOn != null"
              :date-iso-string="data.tradeAccount?.createdOn"
              format="YYYY-MM-DD"
            />
          </div>
        </div>
      </div>

      <div class="d-flex gap-4 mt-4">
        <div class="borders">
          <div class="head">{{ $t("fields.leverage") }}</div>
          <div class="content">
            {{ data.tradeAccount.tradeAccountStatus?.leverage }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.balance") }}</div>
          <div class="content">
            {{ data.tradeAccount.tradeAccountStatus?.balance }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.equity") }}</div>
          <div class="content">
            {{ data.tradeAccount.tradeAccountStatus?.equity }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.margin") }}</div>
          <div class="content">
            {{ data.tradeAccount.tradeAccountStatus?.margin }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.marginLevel") }}</div>
          <div class="content">
            {{ data.tradeAccount.tradeAccountStatus?.marginLevel }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.marginFree") }}</div>
          <div class="content">
            {{ data.tradeAccount.tradeAccountStatus?.marginFree }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.credit") }}</div>
          <div class="content">
            {{ data.tradeAccount.tradeAccountStatus?.credit }}
          </div>
        </div>

        <div class="borders">
          <div class="head">{{ $t("fields.interestRate") }}</div>
          <div class="content">
            {{ data.tradeAccount.tradeAccountStatus?.interestRate }}
          </div>
        </div>
      </div>
    </div>

    <!-- Relationship -->
    <el-divider v-if="data.hasTradeAccount == true" />
    <div class="px-2" v-if="data.hasTradeAccount == true">
      <p class="title">Relationship</p>
      <div>
        <div v-if="data.role != 100" class="d-flex gap-4">
          <div class="borders">
            <div class="head">{{ $t("fields.referCode") }}</div>
            <div class="content">{{ data?.referCode }}</div>
          </div>

          <div class="borders">
            <div class="head">Direct IB ID</div>
            <div class="content">
              {{ data?.agentAccountId }}
            </div>
          </div>

          <div class="borders">
            <div class="head">Direct Sales ID</div>
            <div class="content">
              {{ data?.salesAccountId }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import AccountOverviewService from "@/projects/tenant/views/accountOverview/services/AccountOverviewServices";
import i18n from "@/core/plugins/i18n";
import { ServiceTypes } from "@/core/types/ServiceTypes";
const { t } = i18n.global;
const isLoading = ref(false);
const data = ref<any>([]);
const props = defineProps({
  account: {
    type: Object,
    required: true,
  },
});
const fetchData = async () => {
  if (!props.account || !props.account.id) return;
  isLoading.value = true;
  try {
    const res = await AccountOverviewService.getAccountDetailById(
      props.account.id
    );
    data.value = res;
  } catch (error) {
    console.error("Error fetching data:", error);
  } finally {
    isLoading.value = false;
  }
};
watch(
  () => props.account,
  (newAccount, oldAccount) => {
    if (newAccount && newAccount.id !== oldAccount?.id) {
      fetchData();
    }
  },
  { deep: true }
);
onMounted(() => {
  fetchData();
});
</script>
<style scoped>
.borders {
  text-align: center;
  width: 150px;
  border: 1px solid #ccc;
  padding: 10px;
  margin-left: 2px;
  margin-right: 2px;
  border-radius: 5px;
}
.head {
  font-weight: bold;
  font-size: 14px;
  padding-bottom: 2px;
}
.title {
  font-weight: bold;
  font-size: 16px;
}
</style>
