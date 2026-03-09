<template>
  <LoadingRing />
</template>

<script lang="ts" setup>
import { onMounted } from "vue";
// import { useRouter } from "vue-router";

// const router = useRouter();

const props = defineProps({
  token: {
    type: String,
    required: true,
  },
});

onMounted(async () => {
  const token = props.token;
  if (token) {
    // await store.dispatch(Actions.LOGOUT);
    window.localStorage.clear();

    localStorage.setItem("jwt_token", JSON.stringify({ access_token: token }));
    localStorage.setItem("id_token", token);
    // router.push({ name: "dashboard" });
    window.location.href = "/portal";
  }
});
</script>
