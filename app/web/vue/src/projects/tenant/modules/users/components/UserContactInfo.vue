<template>
  <div>
    <div class="card">
      <div class="card-header">
        <h3 class="card-title">
          {{ $t("title.socialMedia") }}
        </h3>
        <div class="card-toolbar">
          <el-button
            @click="showSocialMediaForm('create')"
            :disabled="isLoading"
          >
            {{ $t("action.addSocialMedia") }}
          </el-button>
        </div>
      </div>
      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.appName") }}</th>
              <th class="">{{ $t("fields.accountId") }}</th>
              <th class="">{{ $t("fields.connectId") }}</th>
              <th class="">{{ $t("fields.staffName") }}</th>
              <th class="text-center min-w-150px">
                {{ $t("action.action") }}
              </th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && socialMedias.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in socialMedias" :key="index">
              <td>{{ $t("fields." + item.name) }}</td>
              <td>{{ item.account }}</td>
              <td>
                {{ item.connectId }}
              </td>
              <td>
                {{ item.staffName }}
              </td>

              <td class="text-center">
                <el-button @click="showSocialMediaForm('edit', item)">
                  {{ $t("action.edit") }}
                </el-button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
  <SocialMediaFormModal
    ref="socialMediaFormRef"
    @application-submitted="fetchData"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, provide } from "vue";
import UserService from "../services/UserService";
import SocialMediaFormModal from "./modal/SocialMediaFormModal.vue";
import { processKeysToCamelCase } from "@/core/services/api.client";

const props = defineProps<{
  partyId: number;
}>();

const isLoading = ref(true);
const socialMedias = ref<any>([]);
const socialMediaFormRef = ref<InstanceType<typeof SocialMediaFormModal>>();

const fetchData = async () => {
  isLoading.value = true;
  try {
    socialMedias.value = processKeysToCamelCase(
      await UserService.getSocialMediaInfo(props.partyId)
    );
    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
};

const showSocialMediaForm = (_type, _item?) => {
  socialMediaFormRef.value?.show(props.partyId, _type, _item);
};

onMounted(async () => {
  await fetchData();
});
</script>
