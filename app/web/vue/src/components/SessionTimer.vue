<template>
  <div v-if="enabled" class="session-timer">
    <el-tag effect="dark" class="tag"
      >{{ $t("fields.elapsed") }}: {{ elapsed }}</el-tag
    >
    <el-tag effect="dark" class="tag" :type="remainingTagType"
      >{{ $t("fields.remaining") }}: {{ remaining }}</el-tag
    >
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { useStore } from "@/store";
import { Actions } from "@/store/enums/StoreEnums";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";

const { t } = i18n.global;
const router = useRouter();
const store = useStore();

const SESSION_KEY = "session_login_time";

const timeoutHours = 10;
const timeoutMs = timeoutHours * 60 * 60 * 1000;
const enabled = timeoutHours > 0;

const elapsed = ref("00:00:00");
const remaining = ref("00:00:00");
const remainingMs = ref(timeoutMs);

let intervalId: ReturnType<typeof setInterval> | null = null;

const remainingTagType = computed(() => {
  const ms = remainingMs.value;
  if (ms <= 15 * 60 * 1000) return "danger";
  if (ms <= 60 * 60 * 1000) return "warning";
  return "info";
});

function formatDuration(ms: number): string {
  if (ms <= 0) return "00:00:00";
  const totalSeconds = Math.floor(ms / 1000);
  const h = Math.floor(totalSeconds / 3600);
  const m = Math.floor((totalSeconds % 3600) / 60);
  const s = totalSeconds % 60;
  return [h, m, s].map((v) => String(v).padStart(2, "0")).join(":");
}

function tick() {
  const loginTime = parseInt(localStorage.getItem(SESSION_KEY) || "0", 10);
  if (!loginTime) return;

  const now = Date.now();
  const elapsedMs = now - loginTime;
  const remMs = timeoutMs - elapsedMs;

  elapsed.value = formatDuration(elapsedMs);
  remaining.value = formatDuration(remMs);
  remainingMs.value = remMs;

  if (remMs <= 0) {
    stop();
    handleLogout();
  }
}

async function handleLogout() {
  await store.dispatch(Actions.LOGOUT);
  await router.push({ name: "sign-in" });
  MsgPrompt.warning(t("tip.sessionExpiredLoginAgain"));
}

function stop() {
  if (intervalId !== null) {
    clearInterval(intervalId);
    intervalId = null;
  }
}

onMounted(() => {
  if (!enabled) return;
  tick();
  intervalId = setInterval(tick, 1000);
});

onUnmounted(() => {
  stop();
});
</script>

<style scoped>
.session-timer {
  display: inline-flex;
  align-items: center;
}

.tag {
  font-size: 15px;
  margin-left: 10px;
  font-variant-numeric: tabular-nums;
  letter-spacing: 0.5px;
}
</style>
