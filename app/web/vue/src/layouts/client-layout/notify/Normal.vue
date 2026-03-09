<template>
  <div
    :id="'notity_normal_' + index"
    class="toast"
    role="alert"
    aria-live="assertive"
    aria-atomic="true"
    data-kt-docs-toast="stack"
  >
    <div class="toast-header">
      <span class="svg-icon svg-icon-2 svg-icon-primary me-3">...</span>
      <strong class="me-auto">Keenthemes</strong>
      <small>11 mins ago</small>
      <button
        type="button"
        class="btn-close"
        data-bs-dismiss="toast"
        aria-label="Close"
      ></button>
    </div>
    <div class="toast-body">Hello, world! This is a toast message.</div>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted } from "vue";
import { Toast } from "bootstrap";
export default defineComponent({
  name: "notify-normal",
  props: {
    index: Number,
    msg: Object,
  },
  components: {},
  setup(props) {
    onMounted(() => {
      console.log(props);
      const _toast = new Toast(
        document.getElementById("notity_normal_" + props.index)
      );
      _toast.show();
    });
    return {};
  },
});
</script>
<style scoped>
.t-top-60 {
  top: 60px;
}
</style>
