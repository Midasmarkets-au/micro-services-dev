<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">Check AWS SES Email</div>
    </div>
    <div class="card-body fs-5">
      <div class="mt-4">
        <el-input
          v-model="emailAddress"
          style="max-width: 600px"
          placeholder="Please input"
          class="input-with-select"
        >
          <template #prepend>Email</template>

          <template #append>
            <el-button
              v-text="'Search'"
              @click="checkEmailAddress"
              :disabled="isLoading"
            />
          </template>
        </el-input>
      </div>
      <div class="mt-4" v-if="checkResult">
        {{ checkResult.email }}: {{ checkResult.status }}
        <el-button
          v-if="checkResult.status"
          type="danger"
          @click="removeEmailAddress(checkResult.email)"
          :loading="isLoading"
          >Remove</el-button
        >
      </div>
      <div class="mt-4" v-if="error">
        {{ error }}
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import SystemService from "@/projects/tenant/modules/system/services/SystemService";
import { ref, onMounted } from "vue";
import { TenantTypes } from "@/core/types/TenantTypes";
import { getCurrentInstance } from "vue";
const isLoading = ref(false);
const data = ref<any>([]);
const totalUsers = ref(0);
const tenantId = ref<any>(null);
const emailAddress = ref("");
const checkResult = ref<any>(null);
const error = ref<any>(null);

var siteColors = {
  au: "#7a9e7a",
  bvi: "#fa6b6c",
  sea: "#349beb",
  mn: "#b497b4",
  jp: "#f6c23e",
};

const getSiteColor = (site: any) => {
  return siteColors[TenantTypes[site]] || "#000000"; // default color if site not found
};

const removeEmailAddress = async (email) => {
  isLoading.value = true;
  try {
    const response = await SystemService.removeEmailSuppression(email);
    error.value = "Removed successfully";
  } catch (e) {
    error.value = "Removed failed: " + e;
  }
  isLoading.value = false;
};

const checkEmailAddress = async () => {
  if (!emailAddress.value) {
    return false;
  }
  checkResult.value = null;
  isLoading.value = true;
  error.value = null;
  try {
    const response = await SystemService.checkEmailSuppression(
      emailAddress.value
    );
    checkResult.value = {
      email: emailAddress.value,
      status: response,
    };
    emailAddress.value = "";
  } catch (e) {
    error.value = "Error: " + e;
  }
  isLoading.value = false;
};

onMounted(() => {
  tenantId.value = localStorage.getItem("tenant");
});
</script>
