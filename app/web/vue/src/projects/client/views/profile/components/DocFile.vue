<template>
  <div class="col-12 col-sm-12 col-xl">
    <div class="card h-100">
      <div v-if="isMobile" class="card-body d-flex align-items-center">
        <img
          :src="`/images/files/${
            {
              [FileFormatTypes.PDF]: 'pdf',
              [FileFormatTypes.JPEG]: 'blank-image',
              [FileFormatTypes.JPG]: 'blank-image',
              [FileFormatTypes.PNG]: 'blank-image',
            }[contentType] ?? 'doc'
          }.svg`"
          alt=""
          style="width: 40px; height: 40px"
        />
        <a
          @click="onImageClick(props.media)"
          class="fs-5 fw-semibold ms-5 text-black text-opacity-75 cursor-pointer text-hover-primary"
        >
          {{ props.media.fileName }}
        </a>
      </div>

      <div
        v-if="!isMobile"
        class="card-body d-flex justify-content-center text-center flex-column p-8"
      >
        <div class="text-gray-800 d-flex flex-column">
          <div class="symbol symbol-60px mb-6">
            <img
              :src="`/images/files/${
                {
                  [FileFormatTypes.PDF]: 'pdf',
                  [FileFormatTypes.JPEG]: 'blank-image',
                  [FileFormatTypes.JPG]: 'blank-image',
                  [FileFormatTypes.PNG]: 'blank-image',
                }[contentType] ?? 'doc'
              }.svg`"
              alt=""
            />
          </div>

          <div
            class="fs-5 fw-semibold mb-2 w-100 overflow-hidden"
            style="white-space: nowrap; text-overflow: ellipsis"
          >
            {{ props.media.fileName }}
          </div>
        </div>

        <a
          v-if="
            [
              VerificationDocumentTypes.IdFront,
              VerificationDocumentTypes.IdBack,
            ].includes(props.media?.documentType)
          "
          @click="onImageClick(props.media)"
          target="_blank"
          class="fs-7 fw-semobold text-gray-400 text-hover-primary mt-auto download-btn mb-3"
        >
          {{ props.createdAt }}
        </a>
        <span
          class="badge badge-danger cursor-pointer"
          @click="showRejectReason"
          v-if="props.media?.status === 3"
        >
          {{ $t("status.docRejected") }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, inject } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import { FileFormatTypes } from "@/core/types/FileFormatTypes";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { VerificationDocumentTypes } from "@/core/types/VerificationInfos";

const emits = defineEmits<{
  (e: "clickDownload", media: any): void;
}>();
const props = defineProps<{
  createdAt: string;
  media: any;
}>();

const { t } = useI18n();

const onImageClick = (_media: any) => {
  emits("clickDownload", _media);
};

const contentType = computed(
  () =>
    ({
      ["application/pdf"]: FileFormatTypes.PDF,
      ["image/jpeg"]: FileFormatTypes.JPEG,
      ["image/jpg"]: FileFormatTypes.JPG,
      ["image/png"]: FileFormatTypes.PNG,
      ["text/plain"]: FileFormatTypes.TXT,
    }[props.media?.contentType])
);

const showConfirmBox = inject(
  ClientGlobalInjectionKeys.OPEN_CONFIRM_MODAL,
  (...args) => MsgPrompt.info()
);

const showRejectReason = () => {
  //\
  showConfirmBox(
    () => null,
    props.media.rejectedReason,
    t("title.rejectReason"),
    undefined,
    undefined,
    true
  );
};
</script>

<style>
.download-btn:hover {
  cursor: pointer;
}
</style>
