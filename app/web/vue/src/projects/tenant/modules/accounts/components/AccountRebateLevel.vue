<template>
  <levelRebateForm
    v-if="!isLoading"
    :rebateType="rebateType"
    :accountDetails="accountDetails"
    :fromRebateRuleDetailId="
      accountDetails.rebateClientRule.rebateDirectSchemaId
    "
    @updateRebateRuleDetailId="updateRebateRuleDetailId"
  ></levelRebateForm>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import AccountService from "../services/AccountService";
import { useRoute } from "vue-router";
import levelRebateForm from "./form/LevelRebateForm.vue";

const props = defineProps<{
  accountDetails: any;
}>();

const isLoading = ref(true);

const accountDetails = ref({});
const rebateType = ref("");
const route = useRoute();

const updateRebateRuleDetailId = (_id: any) => {
  accountDetails.value.rebateClientRule.rebateDirectSchemaId = _id;
};

onMounted(async () => {
  accountDetails.value = props.accountDetails;

  rebateType.value =
    props.accountDetails.rebateClientRule.distributionType.toString();

  if (route.query.accountId) {
    accountDetails.value = await AccountService.getAccountDetailById(
      parseInt(route.query.accountId as string)
    );
  }

  isLoading.value = false;
});
</script>
