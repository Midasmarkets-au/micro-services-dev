<template>
  <div style="width: 392px">
    <a
      @click="handleMouseEnter(props.media)"
      class="d-block overlay mb-4"
      data-fslightbox="lightbox-hot-sales"
      href="#"
    >
      <div
        class="overlay-wrapper card-rounded min-h-225px overflow-hidden h-275px border border-1"
      >
        <template v-if="props.media">
          <div
            v-if="
              [
                FileFormatTypes.JPEG,
                FileFormatTypes.JPG,
                FileFormatTypes.PNG,
              ].includes(contentType as any)
            "
            class="w-100 overlay-wrapper bgi-no-repeat bgi-position-center bgi-size-cover card-rounded h-275px"
            :style="
              contentUrlWithToken !== ''
                ? `background-image: url(${contentUrlWithToken});`
                : ''
            "
          ></div>

          <div
            v-else-if="
              [FileFormatTypes.PDF].includes(contentType as any) &&
              contentUrlWithToken !== ''
            "
          >
            <VuePdfEmbed :source="contentUrlWithToken" class="" />
          </div>
        </template>

        <div
          class="w-100 h-100 d-flex justify-content-center align-items-center bg-gray-200"
          v-else
        >
          <h1
            class="text-black-50 opacity-20 fs-1"
            style="transform: rotate(45deg)"
          >
            User has not uploaded this file yet
          </h1>
        </div>
      </div>

      <div class="overlay-layer bg-dark card-rounded bg-opacity-25">
        <i class="bi bi-eye-fill fs-2x text-white"></i>
      </div>
    </a>
  </div>
</template>

<script setup lang="ts">
import VuePdfEmbed from "vue-pdf-embed";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { inject, onMounted, Ref, ref, watch } from "vue";
import { FileFormatTypes } from "@/core/types/FileFormatTypes";
import { getImageUrl } from "@/core/plugins/ProcessImageLink";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import UserService from "@/projects/tenant/modules/users/services/UserService";

const props = defineProps<{
  title?: string;
  descp?: string;
  media: any;
  mediaType?: string;
  enableDelete?: boolean;
  verificationDetails?: any;
}>();

const emits = defineEmits<{
  (e: "fileDeleted"): void;
}>();

const contentTypeStr = ref(props.media?.contentType);
const contentType = ref(FileFormatTypes.JPEG);

const contentUrlWithToken = ref("");
const getContentUrlWithToken = async () => {
  if (!props.media) return;
  contentUrlWithToken.value = await TenantGlobalService.getImageUrlWithToken(
    getImageUrl(props.media?.guid)
  );
};

watch(() => props.media, getContentUrlWithToken);

const openConfirmModal = inject(InjectKeys.OPEN_CONFIRM_MODAL);

const openRejectModal = inject(
  InjectKeys.OPEN_REJECT_REASON_MODAL,
  (formData: any) => MsgPrompt.error("Get Modal Failed")
);

const handleRejectDocument = () => {
  openRejectModal((formData: any) =>
    UserService.rejectUserDocument(
      props.verificationDetails.id,
      props.media.id,
      formData
    )
      .then(() => MsgPrompt.success("Document Rejected Successfully"))
      .then(() => emits("fileDeleted"))
  );
};

const fileShowRef = inject<Ref>(InjectKeys.FILE_SHOW_REF);

const handleMouseEnter = async (media) => {
  if (!media) return;
  fileShowRef?.value?.show(media);
};

const handleApproveDocument = () => {
  openConfirmModal?.(
    () =>
      UserService.approveUserDocument(
        props.verificationDetails.id,
        props.media.id
      )
        .then(() => MsgPrompt.success("Document Approved Successfully"))
        .then(() => emits("fileDeleted")),
    undefined,
    {
      confirmText: "Confirm to approve this document?",
      confirmColor: "success",
    }
  );
};

const deleteFile = () => {
  openConfirmModal?.(
    () =>
      UserService.deleteVerificationDocForUser(
        props.verificationDetails.id,
        props.media.id
      ).then(() => emits("fileDeleted")),
    undefined,
    {
      confirmText: "Confirm to delete this document?",
      confirmColor: "warning",
    }
  );
};

onMounted(async () => {
  // console.log("media", props.media);
  // console.log("contentType", contentType.value);

  contentType.value =
    {
      ["application/pdf"]: FileFormatTypes.PDF,
      ["image/jpeg"]: FileFormatTypes.JPEG,
      ["image/jpg"]: FileFormatTypes.JPG,
      ["image/png"]: FileFormatTypes.PNG,
      ["text/plain"]: FileFormatTypes.TXT,
    }[contentTypeStr.value] ?? FileFormatTypes.JPEG;

  getContentUrlWithToken();
});
</script>
