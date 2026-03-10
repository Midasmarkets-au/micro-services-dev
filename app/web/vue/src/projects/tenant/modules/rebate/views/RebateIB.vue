<template>
  <!--begin::Row-->
  <div class="row mb-5 mb-xl-10 mx-0">
    {{ $t("tip.tbd") }}
    <img class="testimg" src="../assets/images/work.jpeg" alt="test" style="" />
  </div>
  <!--end::Row-->
</template>

<script lang="ts" setup>
import { ref, inject } from "vue";
import "../assets/css/style.css";

const api = inject("api");
console.log(api);
const items = ref([]);
console.log(items);
</script>
