<template>
  <!--begin::System Layout-->
  <div class="d-flex flex-column flex-root" id="kt_app_root">
    <router-view></router-view>
  </div>
  <!--end::System Layout-->
</template>

<script>
import { defineComponent, onMounted, onUnmounted } from "vue";
import { useStore } from "@/store";
import { Actions } from "@/store/enums/StoreEnums";

export default defineComponent({
  name: "system-layout",
  components: {},
  setup() {
    const store = useStore();

    onMounted(() => {
      console.log("SystemLayout");
      store.dispatch(Actions.ADD_BODY_CLASSNAME, "bgi-size-cover");
      store.dispatch(Actions.ADD_BODY_CLASSNAME, "bgi-position-center");
      store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");

      store.dispatch(Actions.ADD_BODY_CLASSNAME, "bgi-no-repeat");
    });

    onUnmounted(() => {
      store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "bgi-size-cover");
      store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "bgi-position-center");
      store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "bgi-no-repeat");
    });
  },
});
</script>
