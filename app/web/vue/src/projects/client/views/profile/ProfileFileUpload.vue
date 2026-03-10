<template>
  <div class="mb-10" :class="isMobile ? 'px-3' : ''">
    <div>
      <CenterMenu activeMenuItem="fileUpload" />
    </div>
    <div class="card shadow-sm round-bl-br mb-2">
      <!--begin::Content-->
      <div class="card-header d-flex align-items-center">
        <div class="card-title">{{ $t("title.additionalDocuments") }}</div>
        <button
          v-if="verificationInfo.status !== VerificationStatusTypes.Incomplete"
          class="btn btn-sm btn-light-primary btn-bordered"
          @click="openUploadFilePanel()"
        >
          <span>{{ $t("action.uploadFile") }}</span>
        </button>
      </div>
    </div>
    <div class="card round-tl-tr">
      <div
        v-if="verificationInfo.status !== VerificationStatusTypes.Incomplete"
        id="kt_profile_change_password"
        class="card-body"
      >
        <div class="row">
          <div v-if="isLoading" class="d-flex justify-content-center">
            <LoadingRing />
          </div>
          <div
            v-else-if="!isLoading && verificationDocuments.length === 0"
            class="d-flex justify-content-center"
          >
            <NoDataBox />
          </div>
          <div
            v-else
            class="col-lg-2 mt-5"
            v-for="(item, index) in verificationDocuments"
            :key="index"
          >
            <KTFile
              :created-at="$t('action.clickView')"
              :media="item"
              @click-download="openFilePreview(item)"
            ></KTFile>
          </div>
        </div>
      </div>
      <div v-else><VerificationTip /></div>
    </div>
  </div>

  <FileUpload ref="fileUploadRef" @refresh="fetchData" />
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import KTFile from "./components/DocFile.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import FileUpload from "./components/ClientFileUploadModal.vue";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
import VerificationTip from "@/projects/client/components/VerificationTip.vue";
import CenterMenu from "./components/CenterMenu.vue";
import { isMobile } from "@/core/config/WindowConfig";
const isLoading = ref(true);
const fileUploadRef = ref<InstanceType<typeof FileUpload>>();
const verificationInfo = ref<any>({});
const verificationDocuments = ref(Array<any>());

const openUploadFilePanel = () => {
  fileUploadRef.value?.show();
};

const openFileViewModal = inject<(media: any) => void>(
  ClientGlobalInjectionKeys.OPEN_FILE_MODAL
);

const openFilePreview = (media: any) => {
  openFileViewModal?.(media);
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await VerificationService.getVerification();
    verificationInfo.value = res.data;
    verificationDocuments.value = res.data.document ?? [];
    isLoading.value = false;
  } catch (error) {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchData();
});
</script>
<style scoped>
.sub-menu {
  width: 100%;
  white-space: nowrap;
}
</style>
