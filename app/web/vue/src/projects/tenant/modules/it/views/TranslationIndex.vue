<template>
  <div class="card">
    <div class="card-body">
      <div></div>
      <div class="d-flex align-items-center gap-5">
        <el-input
          :disabled="isLoading"
          v-model="clientAddress"
          placeholder="lang folder address"
          class="w-400px"
        ></el-input>
        <el-button
          type="primary"
          :loading="isLoading"
          @click="submitClient"
          plain
          >Update Client</el-button
        >
      </div>

      <el-button
        type="success"
        class="mt-5"
        plain
        :disabled="isLoading"
        v-for="(link, key) in clientLinks"
        :key="key"
        @click="clientChange(link)"
        >{{ key }}</el-button
      >
    </div>
  </div>
  <div class="card mt-10">
    <div class="card-body">
      <div>
        <el-input
          placeholder="specific array key to update"
          :disabled="isLoading"
          v-model="webInput"
          class="w-400px mb-4"
        >
        </el-input>
      </div>
      <div class="d-flex align-items-center gap-5">
        <el-input
          :disabled="isLoading"
          v-model="webAddress"
          placeholder="lang folder address"
          class="w-400px"
        >
        </el-input>

        <el-input
          v-model="webFileName"
          class="w-150px"
          :disabled="isLoading"
        ></el-input>
        <el-button type="warning" :loading="isLoading" @click="submitWeb" plain
          >Update Web</el-button
        >
      </div>

      <el-button
        type="success"
        class="mt-5"
        plain
        :disabled="isLoading"
        v-for="(link, key) in webLinks"
        :key="key"
        @click="webChange(link)"
        >{{ key }}</el-button
      >
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref } from "vue";
import ItServices from "../services/ItServices";

const clientAddress = ref<string>(
  "/Users/kay/Documents/Projects/client/src/locales"
);
const webAddress = ref<string>(
  "/Users/kay/Documents/Projects/web/resources/lang/"
);
const webFileName = ref<string>("theBCR.php");
const webInput = ref<string>("");
const webArray = ref<any>([]);
const isLoading = ref(false);

const submitClient = async () => {
  isLoading.value = true;
  try {
    await ItServices.updateClientTranslation({ address: clientAddress.value });
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const submitWeb = async () => {
  isLoading.value = true;
  try {
    if (webInput.value) {
      webArray.value = webInput.value.trim().split(",");
    }
    await ItServices.updateWebTranslation({
      address: webAddress.value,
      fileName: webFileName.value,
      translateArray: webArray.value.length > 0 ? webArray.value : null,
    });
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const clientLinks = ref({
  "Kay's Client": "/Users/kay/Documents/Projects/client/src/locales",
});

const webLinks = ref({
  "Kay's Web": "/Users/kay/Documents/Projects/web/resources/lang/",
});

const clientChange = (link: string) => {
  clientAddress.value = link;
};

const webChange = (link: string) => {
  webAddress.value = link;
};
</script>
