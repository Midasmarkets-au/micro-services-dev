<template>
  <div v-if="!isLoading">
    <UserDocuments
      :verificationDetails="verificationDetails"
      :verifyOperation="false"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import AccountService from "../services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserDocuments from "../../users/components/UserDocuments.vue";

const props = defineProps<{
  accountId: number;
  partyId: number;
}>();
const verificationDetails = ref({} as any);
const isLoading = ref(true);

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await AccountService.queryVerificationByCriteria({
      PartyId: props.partyId,
    });
    verificationDetails.value = await AccountService.getVerificationById(
      res.data[0].id
    );
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(async () => {
  await fetchData();
});
</script>

<style scoped></style>
