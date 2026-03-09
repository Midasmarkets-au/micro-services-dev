<template>
  <div v-if="isLoading">
    <LoadingRing />
  </div>
  <div v-else>
    <hr />

    <div
      class="row fw-bold fs-5"
      style="background-color: #f5f5f5; padding: 10px"
    >
      <div class="col-2">Role</div>
      <div class="col-2">Group</div>
      <div class="col-2">IB Name</div>
      <div class="col-2">IB Account</div>
      <div class="col-4">Category</div>
    </div>
    <div class="row fs-5" style="padding: 10px">
      <div class="col-2">
        {{ $t("type.accountRole." + accountDetails.role) }}
      </div>
      <div class="col-2">{{ accountDetails.agentAccount.group }}</div>
      <div class="col-2">{{ accountDetails.agentAccount.name }}</div>
      <div class="col-2">
        {{ accountDetails.agentAccount.uid }}({{
          accountDetails.agentAccount.id
        }})
      </div>
      <div class="col-4">--</div>
    </div>
    <div
      class="row fw-bold fs-5"
      style="background-color: #f5f5f5; padding: 10px"
    >
      <div class="col-2">Sales</div>
      <div class="col-2">Sales Account</div>
      <div class="col-2">Sales Code</div>
      <div class="col-2">Terms</div>
      <!-- <div class="col-4">
        <template v-else-if="rebateType == RebateDistributionTypes.Allocation"
          >Rebate Setting</template
        >
      </div> -->
    </div>
    <div class="row fs-5" style="padding: 10px">
      <div class="col-2">{{ accountDetails.salesAccount.name }}</div>
      <div class="col-2">
        {{ accountDetails.salesAccount.uid }}({{
          accountDetails.salesAccount.id
        }})
      </div>
      <div class="col-2">{{ accountDetails.salesAccount.code }}</div>
      <div class="col-2"></div>
      <!-- <div class="col-4">
        <template v-else-if="rebateType == RebateDistributionTypes.Allocation">
          <span class="badge">{{
            pipCommissionDecision.commission == 0
              ? pipCommissionDecision.pips == 0
                ? "No Setting"
                : "Pips"
              : "Commission"
          }}</span>
          =>
          {{
            pipCommissionDecision.commission == 0
              ? pipCommissionDecision.pips == 0
                ? "No Setting"
                : pipCommissionDecision.pips
              : pipCommissionDecision.commission
          }}</template
        >
      </div> -->
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import AccountService from "../services/AccountService";

const isLoading = ref(true);

const props = defineProps<{
  accountDetails: any;
}>();

const accountDetails = ref({});

// const pipCommissionDecision = ref({} as any);

onMounted(async () => {
  const res = await AccountService.queryAccounts({
    id: props.accountDetails?.id,
  });

  accountDetails.value = res.data[0];
  isLoading.value = false;
});
</script>
