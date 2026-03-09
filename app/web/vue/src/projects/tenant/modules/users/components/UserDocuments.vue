<template>
  <div class="d-flex">
    <div
      class="overflow-auto"
      style="flex: 1; overflow-y: auto"
      :style="{
        height: `${props.height ?? '100%'} !important`,
        'border-right': isShowUserInfos && `1px dashed #999`,
      }"
    >
      <div class="d-flex flex-wrap flex-stack mb-6">
        <!--begin::Title-->
        <h1 class="fw-bold my-2 d-flex flex-column">
          <span class="d-flex justify-content-between">
            <span class="me-4">{{ $t("tip.verifyDocuments") }}</span>

            <el-button
              type="primary"
              plain
              @click="openDocumentFileUploadModal"
            >
              <el-icon><DocumentAdd /></el-icon>
              <span class="ms-1">{{ $t("action.upload") }}</span>
            </el-button>

            <template
              v-if="
                verificationDetails?.status !== VerificationStatusTypes.Approved
              "
            >
              <el-button v-if="!isShowUserInfos" @click="isShowUserInfos = true"
                ><span class="fw-bold">{{ $t("action.showUserInfo") }}</span>
                <el-icon><ArrowRightBold /></el-icon>
              </el-button>
              <el-button
                v-if="isShowUserInfos"
                @click="isShowUserInfos = false"
              >
                <span class="fw-bold">{{ $t("action.hideUserInfo") }}</span
                ><el-icon><ArrowLeftBold /></el-icon>
              </el-button>
            </template>
          </span>
        </h1>
      </div>

      <div
        class="justify-content-between"
        :class="{
          'd-flex': !isShowUserInfos,
        }"
      >
        <UserImageIdsDoc
          :medias="verificationDocuments.idFront"
          :title="$t('tip.idFront')"
          :context="verificationDocuments.idFront[0]?.context"
          :descp="isShowUserInfos ? '' : $t('tip.idRequirement')"
          :verification-details="verificationDetails"
          @file-deleted="fileUploaded"
        />

        <UserImageIdsDoc
          :medias="verificationDocuments.idBack"
          :title="$t('tip.idBack')"
          :context="verificationDocuments.idBack[0]?.context"
          :descp="isShowUserInfos ? '' : $t('tip.idRequirement')"
          :verification-details="verificationDetails"
          @file-deleted="fileUploaded"
        />

        <UserImageIdsDoc
          :medias="verificationDocuments.addressDocument"
          :title="$t('tip.addressDocument')"
          :context="verificationDocuments.addressDocument[0]?.context"
          :descp="isShowUserInfos ? '' : $t('tip.addressRequirement')"
          :verification-details="verificationDetails"
          @file-deleted="fileUploaded"
        />
      </div>

      <hr v-if="verificationDocuments.others.length > 0" class="mb-11" />

      <div
        v-if="
          verificationDocuments.others.length > 0 ||
          Object.keys(finalizeKyc).length != 0
        "
      >
        <div class="d-flex gap-4">
          <h1 class="fw-bold mb-6">{{ $t("tip.additionalDocument") }}</h1>
        </div>

        <div
          v-if="verificationDocuments.others.length > 0 && !isLoading"
          class="flex-wrap"
          :class="{
            'd-flex': !isShowUserInfos,
          }"
        >
          <UserImageDoc
            v-for="(item, index) in verificationDocuments.others"
            :key="index"
            class="me-3"
            :title="item.context"
            :media="item"
            @file-deleted="fileUploaded"
            :verification-details="verificationDetails"
            enable-delete
          />
        </div>

        <div
          v-if="Object.keys(finalizeKyc).length != 0 && !isLoading"
          style="width: 392px"
        >
          <a
            @click="showFinalizedKyc"
            class="d-block overlay mb-4"
            data-fslightbox="lightbox-hot-sales"
            href="#"
          >
            <div
              class="overlay-wrapper card-rounded min-h-225px overflow-hidden h-275px border border-1"
            >
              <div
                class="w-100 h-100 d-flex justify-content-center align-items-center bg-gray-200"
              >
                <h1 class="text-black-50 opacity-80 fs-1">KYC Form</h1>
              </div>
            </div>
            <div class="overlay-layer bg-dark card-rounded bg-opacity-25">
              <i class="bi bi-eye-fill fs-2x text-white"></i>
            </div>
          </a>
        </div>
      </div>

      <div v-if="historicalDocuments.length > 0 && !isLoading">
        <div class="d-flex gap-4">
          <h1 class="fw-bold mb-6">Historical Documents</h1>
        </div>

        <div
          class="flex-wrap"
          :class="{
            'd-flex': !isShowUserInfos,
          }"
        >
          <UserImageDoc
            v-for="(item, index) in historicalDocuments"
            :key="index"
            class="me-3"
            :title="item.context"
            :media="item"
            @file-deleted="fileUploaded"
            :verification-details="verificationDetails"
            :disabledAction="true"
            enable-delete
          />
        </div>
      </div>
    </div>
    <div
      class=""
      v-if="isShowUserInfos"
      style="flex: 1; height: 625px; overflow-y: auto"
      :style="{
        flex: isShowUserInfos ? '1.5' : '0',
        padding: '20px',
      }"
    >
      <UserPersonalInfos :verification-details="verificationDetails" />
    </div>
    <DocumentFileUploadModal
      ref="documentFileUploadModal"
      @refresh="fileUploaded"
    />
    <FinalizedKycModal ref="finalizedKycModal" />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import UserImageDoc from "./UserImageDoc.vue";
import UserPersonalInfos from "@/projects/tenant/modules/users/components/userDocument/UserPersonalInfos.vue";
import {
  VerificationDocumentTypes,
  VerificationStatusTypes,
} from "@/core/types/VerificationInfos";
import {
  ArrowLeftBold,
  ArrowRightBold,
  DocumentAdd,
} from "@element-plus/icons-vue";
import DocumentFileUploadModal from "@/projects/tenant/modules/users/components/modal/DocumentFileUploadModal.vue";
import UserImageIdsDoc from "@/projects/tenant/modules/users/components/UserImageIdsDoc.vue";
import UserService from "../services/UserService";
import { VerificationTypes } from "@/core/types/VerificationInfos";
import FinalizedKycModal from "./modal/FinalizedKycModal.vue";

const emits = defineEmits<{
  (e: "updateVerificationDetails"): void;
}>();

const props = defineProps<{
  verificationDetails: any;
  verifyOperation?: boolean;
  height?: string;
  partyId?: number;
}>();

const isLoading = ref(false);
const historicalDocuments = ref<Array<any>>([]);
const isShowUserInfos = ref(false);
const finalizeKyc = ref({} as any);

const documentFileUploadModal =
  ref<InstanceType<typeof DocumentFileUploadModal>>();
const finalizedKycModal = ref<InstanceType<typeof FinalizedKycModal>>();

const verificationDocuments = ref<Record<string, Array<any>>>({
  idFront: [],
  idBack: [],
  addressDocument: [],
  others: [],
});

const openDocumentFileUploadModal = () => {
  documentFileUploadModal.value?.show(props.verificationDetails.id);
};

const showFinalizedKyc = () => {
  finalizedKycModal.value?.show(finalizeKyc.value);
};

const getKycForm = async () => {
  try {
    const res = await UserService.queryVerifications({
      type: VerificationTypes.KycForm,
      status: VerificationStatusTypes.Approved,
      partyId: props.partyId,
    });
    if (res.data.length != 0) {
      finalizeKyc.value = await UserService.getKycForm(props.partyId);
    }
  } catch (e) {
    console.log(e);
  }
};

const initVerificationDocuments = async (_verificationDetails?: any) => {
  verificationDocuments.value = {
    idFront: [],
    idBack: [],
    addressDocument: [],
    others: [],
  };
  (_verificationDetails ?? props.verificationDetails)?.document?.forEach(
    (doc) =>
      ((
        {
          [VerificationDocumentTypes.IdFront]: () =>
            verificationDocuments.value.idFront.push(doc),
          [VerificationDocumentTypes.IdBack]: () =>
            verificationDocuments.value.idBack.push(doc),
          [VerificationDocumentTypes.Address]: () =>
            verificationDocuments.value.addressDocument.push(doc),
        }[doc.documentType] ??
        (() => verificationDocuments.value.others.push(doc))
      )())
  );

  Object.values(verificationDocuments.value).forEach((arr) => {
    arr.sort((x, y) => y.id - x.id);
  });

  try {
    const mediaList = ref([] as any);
    if (props.partyId != undefined && props.partyId != null) {
      mediaList.value = await UserService.getClientMediaList({
        partyId: props.partyId,
      });
    }

    const uniqueIds = [] as any;
    const verificationDoc = (_verificationDetails ?? props.verificationDetails)
      ?.document;

    if (verificationDoc?.length > 0) {
      for (const item of verificationDoc) {
        uniqueIds.push(item.guid);
      }
    }

    mediaList.value.data?.forEach((item) => {
      if (!uniqueIds.includes(item.guid)) {
        historicalDocuments.value.push(item);
      }
    });
  } catch (e) {
    console.log(e);
  }
};

const fileUploaded = async () => {
  emits("updateVerificationDetails");
};

onMounted(() => {
  initVerificationDocuments();
  if (props.partyId != undefined) getKycForm();
});
</script>

<style scoped></style>
