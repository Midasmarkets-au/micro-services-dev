<template>
  <router-view></router-view>
</template>

<script>
import { defineComponent, onMounted, onUnmounted } from "vue";
import { useStore } from "@/store";
import { Actions } from "@/store/enums/StoreEnums";
import { useRouter } from "vue-router";

export default defineComponent({
  name: "auth-layout",
  components: {},
  setup() {
    const store = useStore();
    const router = useRouter();

    onMounted(() => {
      if (router.currentRoute.value.name === "change-account-password") {
        store.dispatch(Actions.ADD_BODY_CLASSNAME, "app-blank");
        return;
      }
      if (store.getters.isUserAuthenticated) {
        // if (!store.getters.isUser2fa) {
        //   router.push({ name: "2fa" });
        // } else {
        //   router.push({ name: "dashboard" });
        // }
        router.push({ name: "dashboard" });
      }
      // store.dispatch(Actions.ADD_BODY_CLASSNAME, "bg-body");
      store.dispatch(Actions.ADD_BODY_CLASSNAME, "app-blank");
    });

    onUnmounted(() => {
      // store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "bg-body");
    });
  },
});
</script>
