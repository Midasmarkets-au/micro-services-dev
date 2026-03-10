<template>
  <div style="width: 75px">
    <a
      @click="handleMouseEnter(props.guid)"
      class="d-block overlay mb-4"
      data-fslightbox="lightbox-hot-sales"
      href="#"
    >
      <div
        class="overlay-wrapper card-rounded h-75px overflow-hidden border border-1"
      >
        <template v-if="props.guid">
          <div
            class="w-100 h-100 overlay-wrapper bgi-no-repeat bgi-position-center bgi-size-cover card-rounded"
            :style="
              contentUrlWithToken !== ''
                ? `background-image: url(${contentUrlWithToken});`
                : ''
            "
          ></div>
        </template>
      </div>

      <div class="overlay-layer bg-dark card-rounded bg-opacity-25">
        <i class="bi bi-eye-fill fs-2x text-white"></i>
      </div>
    </a>
  </div>
</template>

<script setup lang="ts">
import { inject, onMounted, Ref, ref, watch } from "vue";
import InjectKeys from "@/core/types/ClientGlobalInjectionKeys";
import SupportService from "../../services/SupportService";
const props = defineProps<{
  guid: string;
}>();

const emits = defineEmits<{
  (e: "fileDeleted"): void;
}>();

const media = ref<any>({});
const contentUrlWithToken = ref("");
const getContentUrlWithToken = async () => {
  if (!props.guid) return;
  contentUrlWithToken.value = await SupportService.getImagesWithGuid(
    props.guid
  );
};

watch(() => props.guid, getContentUrlWithToken);

const fileShowRef = inject<Ref>(InjectKeys.IMG_SHOW_REF);

const handleMouseEnter = async (guid) => {
  if (!guid) return;
  media.value = { guid: guid, contentType: "image/jpeg" };
  fileShowRef?.value?.show(media.value);
};

onMounted(async () => {
  getContentUrlWithToken();
});
</script>
