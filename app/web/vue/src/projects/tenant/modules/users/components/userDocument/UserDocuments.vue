<template>
  <div v-if="isLoading" class="d-flex justify-content-center mt-19">
    <LoadingRing />
  </div>
  <div v-else>
    <div class="mb-7 d-flex justify-content-end">
      <el-button type="primary" plain @click="openDocumentFileUploadModal">
        <el-icon><DocumentAdd /></el-icon>
        <span>Upload</span>
      </el-button>
    </div>

    <div class="d-flex flex-wrap">
      <UserImageDoc
        v-for="(item, index) in data"
        :key="index"
        class="me-3"
        :title="item.context"
        :media="item"
        :verification-details="verificationDetails"
      />
    </div>
  </div>
  <DocumentFileUploadModal ref="documentFileUploadModal" @refresh="fetchData" />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import UserImageDoc from "./UserImageDoc.vue";
import UserService from "../../services/UserService";
import DocumentFileUploadModal from "@/projects/tenant/modules/users/components/modal/DocumentFileUploadModal.vue";
import { DocumentAdd } from "@element-plus/icons-vue";
import LoadingRing from "@/components/LoadingRing.vue";

const props = defineProps<{
  verificationDetails: any;
  verifyOperation?: boolean;
  height?: string;
}>();

const data = ref<any>([]);
const isLoading = ref(false);
const documentFileUploadModal =
  ref<InstanceType<typeof DocumentFileUploadModal>>();

const fetchData = async () => {
  isLoading.value = true;

  try {
    const res = await UserService.getClientMediaList({
      partyId: props.verificationDetails.partyId,
    });
    data.value = res.data;
  } catch (e) {
    console.log(e);
  }

  isLoading.value = false;
};

const openDocumentFileUploadModal = () => {
  documentFileUploadModal.value?.show(props.verificationDetails.id);
};

onMounted(() => {
  fetchData();
});
</script>
