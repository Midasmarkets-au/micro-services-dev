<template>
  <div class="mb-10">
    <button class="btn btn-primary" @click="getData">Get Untranslated</button>
    将下面的json
    object翻译成如下几种语言：印尼语，日语，蒙古语，马来语，泰语，越南语，简体中文，繁体中文，每种语言一种一种返回，而且都返回一样的json
    object：

    <div class="row">
      <div class="col-lg-6 mt-5">
        <label class="required" for="">Input json</label>
        <el-input
          type="textarea"
          :rows="10"
          v-model="jsonInput"
          placeholder="Input json"
        ></el-input>
      </div>

      <div class="col-lg-6 mt-5">
        <label class="required" for="">Input json</label>
        <div class="bg-primary">{{ pendingTranslateObject }}</div>
      </div>
    </div>
  </div>
  <button class="btn btn-primary" @click="writeBackToJson">Write Back</button>

  <div>
    <h1>Merge Json Object</h1>
    <el-menu
      :default-active="activeLang"
      class="el-menu-demo mb-5"
      mode="horizontal"
      @select="handleLangSelect"
    >
      <el-menu-item v-for="lang in languages" :index="lang" :key="lang">{{
        lang
      }}</el-menu-item>
    </el-menu>
    <button class="btn btn-primary" @click="mergeJsonHandler">Merge</button>
    <div class="row">
      <div class="col-lg-6 mt-5">
        <label class="required" for="">Original Lang Json</label>
        <el-input
          type="textarea"
          :rows="10"
          v-model="mergeJson1"
          placeholder="Input json"
        ></el-input>
      </div>

      <div class="col-lg-6 mt-5">
        <label class="required" for="">Newly Translated</label>
        <el-input
          type="textarea"
          :rows="10"
          v-model="mergeJson2"
          placeholder="Input json"
        ></el-input>
      </div>

      <div class="col-lg-12 mt-5">
        <label class="required" for="">Merged</label>
        <el-input
          type="textarea"
          :rows="10"
          v-model="mergedJson"
          placeholder="Input json"
        ></el-input>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import Clipboard from "clipboard";
const jsonInput = ref("");
const pendingTranslateObject = ref({});

const mergeJson1 = ref("");
const mergeJson2 = ref("");
const mergedJson = ref("");

const languages = ref<Array<string>>([]);

const allLangDataObjects = ref<any>({});

const activeLang = ref("en-us");
const handleLangSelect = (lang) => {
  activeLang.value = lang;
  console.log(allLangDataObjects.value);
  mergeJson1.value = JSON.stringify(allLangDataObjects.value[lang]);
};

const mergeJsonHandler = () => {
  const obj1 = JSON.parse(mergeJson1.value);
  const obj2 = JSON.parse(mergeJson2.value);
  const strVal = JSON.stringify(deepMerge(obj1, obj2));
  mergedJson.value = strVal;
  Clipboard.copy(strVal);
};

const getData = () => {
  //

  const jsonFiles = require.context("@/locales", true, /\.json$/);
  const fileKeys = jsonFiles.keys();
  languages.value = fileKeys.map((key) =>
    key.replace(/^\.\//, "").replace(/\.json$/, "")
  );
  const jsonFileData = {};

  fileKeys.forEach((key) => {
    const fileName = key.replace(/^\.\//, "").replace(/\.json$/, "");
    jsonFileData[fileName] = jsonFiles(key);
  });

  const { untranslatedMap } = findUntranslatedKeys(
    jsonFileData["en-us"],
    jsonFileData
  );
  // console.log(untranslatedKeys);
  // console.log(untranslatedMap);
  const pendingTranslationObj = createPendingTranslateObject(untranslatedMap);
  pendingTranslateObject.value = pendingTranslationObj;
  console.log(pendingTranslationObj);
  mergeJson2.value = JSON.stringify(pendingTranslationObj);

  const str =
    "将下面的json object翻译成如下几种语言：印尼语，日语，蒙古语，马来语，泰语，越南语，简体中文，繁体中文，每种语言一种一种返回，而且都返回一样的json object：\n\n" +
    mergeJson2.value;
  Clipboard.copy(str);
};

function findUntranslatedKeys(enUsData, translationDatas) {
  const untranslatedKeys = Array<any>();
  const untranslatedMap = new Map();

  function traverseObject(obj, translationObj, path = "") {
    for (const key in obj) {
      const currentPath = path ? `${path}.${key}` : key;
      const enUsValue = obj[key];
      const translationValue = translationObj[key];
      // if (currentPath === "fields.nativeName") {
      //   console.log(currentPath);
      // }
      if (
        typeof enUsValue === "object" &&
        typeof translationValue === "object"
      ) {
        traverseObject(enUsValue, translationValue, currentPath);
      } else if (
        typeof enUsValue !== "object" &&
        (translationValue === undefined ||
          /~/.test(translationValue) ||
          translationValue === "" ||
          translationValue === null)
      ) {
        untranslatedMap.set(currentPath, getEngByKey(enUsData, currentPath));
        untranslatedKeys.push(currentPath);
      }
    }
  }

  Object.keys(translationDatas).map((key) => {
    if (key === "en-us") return;
    const langData = translationDatas[key];
    traverseObject(enUsData, langData);
  });

  return { untranslatedKeys, untranslatedMap };
}

function getEngByKey(engLang: object, engKey: string) {
  const keys = engKey.split(".");

  //
  function getValueFromObj(obj: object, idx: number) {
    if (idx >= keys.length) {
      throw new Error("Length exceed");
    }
    const key = keys[idx];
    if (typeof obj[key] === "string") return obj[key];
    if (typeof obj[key] === "object") return getValueFromObj(obj[key], idx + 1);
    throw new Error("Not found, please check code");
  }

  return getValueFromObj(engLang, 0);
}

function createPendingTranslateObject(untranslatedMap: Map<string, string>) {
  const pendingTranslateObject = {};

  function helper(
    obj: object,
    value: string,
    idx: number,
    keys: Array<string>
  ) {
    const key = keys[idx];
    if (!obj[key]) {
      obj[key] = {};
    }
    if (idx === keys.length - 1) {
      obj[keys[idx]] = value;
      return;
    }
    helper(obj[key], value, idx + 1, keys);
  }

  untranslatedMap.forEach((translation, keyString) => {
    const keys = keyString.split(".");
    helper(pendingTranslateObject, translation, 0, keys);
  });
  return pendingTranslateObject;
}

function isObject(item) {
  return item && typeof item === "object" && !Array.isArray(item);
}

function deepMerge(obj1, obj2) {
  let target = {};
  for (let key in obj1) {
    if (isObject(obj1[key])) {
      if (key in obj2 && isObject(obj2[key])) {
        target[key] = deepMerge(obj1[key], obj2[key]);
      } else {
        target[key] = obj1[key];
      }
    } else if (obj1[key] !== "" || (key in obj2 && obj2[key] === "")) {
      target[key] = obj1[key];
    }
  }

  for (let key in obj2) {
    if (!(key in obj1)) {
      target[key] = obj2[key];
    } else if (
      !isObject(obj2[key]) &&
      obj2[key] !== "" &&
      (obj1[key] === "" || !isObject(obj1[key]))
    ) {
      target[key] = obj2[key];
    }
  }

  return target;
}

const writeBackToJson = () => {
  // eslint-disable-next-line @typescript-eslint/no-var-requires
  // const fs = require("fs");
  // console.log("write back to json");
};

onMounted(() => {
  // getData();
  const jsonFiles = require.context("@/locales", true, /\.json$/);
  languages.value = jsonFiles
    .keys()
    .map((key) => key.replace(/^\.\//, "").replace(/\.json$/, ""));

  jsonFiles.keys().forEach((key) => {
    const fileName = key.replace(/^\.\//, "").replace(/\.json$/, "");
    allLangDataObjects.value[fileName] = jsonFiles(key);
  });
});
</script>

<style scoped></style>
