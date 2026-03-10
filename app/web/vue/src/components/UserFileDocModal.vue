<template>
  <transition name="fade">
    <div v-if="showImageModal" class="showImageModalBackDrop" @click="hide">
      <div class="pdf-preview" @click.stop>
        <div class="pdf-wrap">
          <template v-if="!isLoading">
            <div
              class="content-embedded"
              :style="transform"
              v-if="
                [
                  FileFormatTypes.JPEG,
                  FileFormatTypes.JPG,
                  FileFormatTypes.PNG,
                ].includes(contentType)
              "
            >
              <img :src="src" style="width: 800px" />
            </div>

            <div
              @click.stop
              v-if="[FileFormatTypes.PDF].includes(contentType)"
              :style="transform"
              class="content-embedded"
            >
              <VuePdfEmbed :source="src" :page="state.page" />
            </div>

            <div
              class="content-embedded"
              v-if="[FileFormatTypes.HTML].includes(contentType)"
              v-html="media?.content"
              :style="transform"
            ></div>
          </template>
        </div>

        <div class="page-tool">
          <div class="page-tool-item" @click="pageZoomIn">
            <i class="fa-solid fa-minus fa-xl" style="color: #ffffff"></i>
          </div>
          <template v-if="[FileFormatTypes.PDF].includes(contentType)">
            <div class="page-tool-item" @click="prevPage">Prev</div>
            <div class="page-tool-item">
              {{ state.page }}/{{ state.numPages }}
            </div>
            <div class="page-tool-item" @click="nextPage">Next</div>
          </template>
          <div class="page-tool-item" @click="pageZoomOut">
            <i class="fa-solid fa-plus fa-xl" style="color: #ffffff"></i>
          </div>

          <div class="page-tool-item" @click="rotateRight">
            <i
              class="fa-solid fa-rotate-right fa-xl"
              style="color: #ffffff"
            ></i>
          </div>
        </div>

        <div class="page-tool-top-right" @click="hide">
          <div class="page-tool-item">
            <i class="fa-solid fa-xmark fa-xl" style="color: #ffffff"></i>
          </div>
        </div>

        <div class="page-tool-left">
          <div class="page-tool-item">Drag to move</div>
        </div>

        <div class="page-tool-right">
          <div class="page-tool-item">
            <a :href="src" target="_blank">
              <i class="fa-solid fa-download" style="color: #ffffff"></i>
            </a>
          </div>
        </div>
      </div>
    </div>
  </transition>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, nextTick } from "vue";
import {
  getImageUrl,
  getImageUrlWithToken,
} from "@/core/plugins/ProcessImageLink";
import {
  FileFormatTypes,
  getContentTypeFromMedia,
} from "@/core/types/FileFormatTypes";
import VuePdfEmbed from "vue-pdf-embed";
import { createLoadingTask } from "vue3-pdfjs";
import interact from "interactjs";
import ImageWithToken from "@/components/ImageWithToken.vue";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";

const src = ref("");
const showImageModal = ref(false);
const media = ref<any>(null);

const contentType = ref<FileFormatTypes>(FileFormatTypes.JPEG);

const initStateValue = {
  x: 0,
  y: 0,
  page: 1,
  scale: 1,
  rotate: 0,
  numPages: 0,
};

const state = ref({ ...initStateValue });
const isLoading = ref(true);
const show = async (_media: any) => {
  showImageModal.value = true;
  isLoading.value = true;
  state.value = { ...initStateValue };
  media.value = _media;
  contentType.value = getContentTypeFromMedia(_media);
  src.value = await TenantGlobalService.getImageUrlWithToken(
    getImageUrl(_media.guid)
  );

  if (contentType.value === FileFormatTypes.PDF) {
    const pdfLoadingTask = createLoadingTask(src.value);
    pdfLoadingTask.promise.then((pdf: { numPages: number }) => {
      state.value.numPages = pdf.numPages;
    });
  }
  await nextTick();
  isLoading.value = false;
};

const hide = () => {
  showImageModal.value = false;
  nextTick().then(() => (isLoading.value = true));
};

const transform = computed(
  () =>
    `transform:
          translate(${state.value.x}px, ${state.value.y}px)
          scale(${state.value.scale})
          rotate(${state.value.rotate}deg)
          !important;`
);

const prevPage = () => state.value.page > 1 && (state.value.page -= 1);
const nextPage = () =>
  state.value.page < state.value.numPages && (state.value.page += 1);

const pageZoomOut = () => state.value.scale < 2 && (state.value.scale += 0.1);
const pageZoomIn = () => state.value.scale > 0.5 && (state.value.scale -= 0.1);

const rotateRight = () =>
  (state.value.rotate = (state.value.rotate + 90) % 360);

const registerDragAction = () =>
  interact(".content-embedded").draggable({
    listeners: {
      move(event) {
        let target = event.target;
        let x = (parseFloat(target.getAttribute("data-x")) || 0) + event.dx;
        let y = (parseFloat(target.getAttribute("data-y")) || 0) + event.dy;
        state.value.x = x;
        state.value.y = y;

        target.setAttribute("data-x", x);
        target.setAttribute("data-y", y);
      },
    },
  });

onMounted(() => {
  registerDragAction();
});

defineExpose({
  show,
  hide,
});
</script>

<style scoped lang="scss">
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.showImageModalBackDrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 3060;
  transition: all 0.3s ease-in-out;

  .pdf-preview {
    position: relative;
    padding: 20px 0;

    .pdf-wrap {
      position: relative;
      width: 900px;
      height: 900px;

      //object-fit: cover;
      .content-embedded {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        text-align: center;

        //transform-origin: top left;
        transform-origin: center;
        //border: 1px solid #e5e5e5;
        margin: 0 auto;
      }
    }

    .page-tool,
    .page-tool-left,
    .page-tool-top-right,
    .page-tool-right {
      position: absolute;
      padding-left: 15px;
      padding-right: 15px;
      display: flex;
      align-items: center;
      background: rgb(66, 66, 66);
      color: white;
      border-radius: 19px;
      z-index: 1060;
      cursor: pointer;
      opacity: 0.5;

      .page-tool-item {
        padding: 8px 10px 8px 10px;
        cursor: pointer;
      }
    }

    .page-tool {
      bottom: 35px;

      margin-left: 50%;
      transform: translateX(-50%);
    }

    .page-tool-right {
      bottom: 35px;

      right: 35px;
    }

    .page-tool-left {
      bottom: 35px;
      left: 35px;
    }

    .page-tool-top-right {
      top: 35px;
      right: 20px;
      padding: 0;
      border-radius: 50%;
      width: 40px;
      height: 40px;
      display: flex;
      justify-content: center;
      align-items: center;

      .page-tool-item {
        padding: 0;
      }
    }
  }
}
</style>
