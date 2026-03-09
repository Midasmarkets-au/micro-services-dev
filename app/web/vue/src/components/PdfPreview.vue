<template>
  <!--  <embed :src="state.src" type="application/pdf" width="500px" />-->
  <!--  <embed width="600px" :src="state.src" type="application/pdf" />-->
  <div class="pdf-preview" v-if="true">
    <div class="pdf-wrap">
      <vue-pdf-embed
        :source="state.src"
        :style="scale"
        class="vue-pdf-embed"
        :page="state.page"
      />
    </div>
  </div>
</template>
<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import VuePdfEmbed from "vue-pdf-embed";
import { createLoadingTask } from "vue3-pdfjs";
import interact from "interactjs";

const props = defineProps<{
  src: string;
}>();

const state = ref({
  src: props.src,
  page: 1,
  scale: 1,
  numPages: 0,
});

const scale = computed(() => `transform:scale(${state.value.scale})`);
const prevPage = () => {
  if (state.value.page > 1) {
    state.value.page -= 1;
  }
};
const nextPage = () => {
  if (state.value.page < state.value.numPages) {
    state.value.page += 1;
  }
};
const pageZoomOut = () => {
  if (state.value.scale < 2) {
    state.value.scale += 0.1;
  }
};
const pageZoomIn = () => {
  if (state.value.scale > 0.5) {
    state.value.scale -= 0.1;
  }
};

onMounted(() => {
  //
  const pdfLoadingTask = createLoadingTask(state.value.src);
  pdfLoadingTask.promise.then((pdf: { numPages: number }) => {
    state.value.numPages = pdf.numPages;
  });

  interact(".vue-pdf-embed").draggable({
    listeners: {
      move(event) {
        if (!event.altKey) {
          return;
        }
        let target = event.target;
        let x = (parseFloat(target.getAttribute("data-x")) || 0) + event.dx;
        let y = (parseFloat(target.getAttribute("data-y")) || 0) + event.dy;

        target.style.transform = `translate(${x}px, ${y}px) scale(${state.value.scale})`;

        target.setAttribute("data-x", x);
        target.setAttribute("data-y", y);
      },
    },
  });
});
</script>

<style lang="scss" scoped>
.pdf-preview {
  position: relative;
  padding: 20px 0;

  .pdf-wrap {
    position: relative;
    width: 900px;
    height: 920px;

    //object-fit: cover;
    .vue-pdf-embed {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;

      text-align: center;
      //transform-origin: top left;
      transform-origin: center;
      border: 1px solid #e5e5e5;
      margin: 0 auto;
    }
  }

  .page-tool,
  .page-tool-left,
  .page-tool-right {
    position: absolute;
    padding-left: 15px;
    padding-right: 15px;
    bottom: 35px;
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
    margin-left: 50%;
    transform: translateX(-50%);
  }

  .page-tool-right {
    right: 35px;
  }

  .page-tool-left {
    left: 35px;
  }
}
</style>
