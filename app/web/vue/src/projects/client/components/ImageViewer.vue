<template>
  <transition name="fade">
    <div v-if="showImageModal" class="showImageModalBackDrop" @click="hide">
      <div class="pdf-preview" @click.stop>
        <div class="pdf-wrap">
          <template v-if="!isLoading">
            <div v-for="item in media" :key="item.guid">
              <div
                :data-guid="item.guid"
                class="content-embedded draggable-element"
                :style="transform"
              >
                <img :src="src" style="width: 800px" />
              </div>
            </div>
          </template>
        </div>

        <div class="page-tool">
          <div class="page-tool-item" @click="pageZoomIn">
            <i class="fa-solid fa-minus fa-xl" style="color: #ffffff"></i>
          </div>
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
import interact from "interactjs";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";

const src = ref("");
const showImageModal = ref(false);
const media = ref([]);

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
  console.log("show", _media);
  showImageModal.value = true;
  isLoading.value = true;
  state.value = { ...initStateValue };
  media.value.push(_media);
  console.log("media", media.value);
  src.value = await ClientGlobalService.getImagesWithGuid(_media.guid);

  await nextTick();
  const uniqueSelector = `.draggable-element[data-guid="${media.value.guid}"]`;
  registerDragAction(uniqueSelector);
  isLoading.value = false;
};

const hide = () => {
  showImageModal.value = false;
  nextTick().then(() => (isLoading.value = true));
};

const pageZoomOut = () => state.value.scale < 2 && (state.value.scale += 0.1);
const pageZoomIn = () => state.value.scale > 0.5 && (state.value.scale -= 0.1);

const rotateRight = () =>
  (state.value.rotate = (state.value.rotate + 90) % 360);

const registerDragAction = (uniqueSelector) =>
  interact(uniqueSelector).draggable({
    listeners: {
      move(event) {
        let target = event.target;
        let dx = (parseFloat(target.getAttribute("data-x")) || 0) + event.dx;
        let dy = (parseFloat(target.getAttribute("data-y")) || 0) + event.dy;

        // Update state
        state.value.x = dx;
        state.value.y = dy;

        // Update attributes for persistence
        target.setAttribute("data-x", dx);
        target.setAttribute("data-y", dy);
      },
    },
  });

const transform = computed(
  () =>
    `transform:
          translate(${state.value.x}px, ${state.value.y}px)
          scale(${state.value.scale})
          rotate(${state.value.rotate}deg)
          !important;`
);

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
