// wwwroot/js/voice.js
// Voice recognition for ChatWidget - Web Speech API integration

window.VoiceRecognition = {
    recognition: null,
    isRecording: false,
    dotNetRef: null,

    // Check if speech recognition is supported
    isSupported: function () {
        return 'webkitSpeechRecognition' in window || 'SpeechRecognition' in window;
    },

    // Initialize speech recognition with Blazor callback reference
    initialize: function (dotNetReference) {
        this.dotNetRef = dotNetReference;

        if (!this.isSupported()) {
            console.warn('Speech recognition not supported in this browser');
            return false;
        }

        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        this.recognition = new SpeechRecognition();

        // Configuration
        this.recognition.continuous = false;
        this.recognition.interimResults = true;
        this.recognition.lang = 'en-AU'; // Australian English
        this.recognition.maxAlternatives = 1;

        // Event handlers
        this.recognition.onstart = () => {
            this.isRecording = true;
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnVoiceStart');
            }
        };

        this.recognition.onresult = (event) => {
            let interimTranscript = '';
            let finalTranscript = '';

            for (let i = event.resultIndex; i < event.results.length; i++) {
                const transcript = event.results[i][0].transcript;
                if (event.results[i].isFinal) {
                    finalTranscript += transcript;
                } else {
                    interimTranscript += transcript;
                }
            }

            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnVoiceResult',
                    finalTranscript || interimTranscript,
                    finalTranscript !== ''
                );
            }
        };

        this.recognition.onerror = (event) => {
            this.isRecording = false;
            let errorMessage = 'Voice recognition error';

            switch (event.error) {
                case 'no-speech':
                    errorMessage = 'No speech detected. Please try again.';
                    break;
                case 'audio-capture':
                    errorMessage = 'No microphone found. Please check your settings.';
                    break;
                case 'not-allowed':
                    errorMessage = 'Microphone access denied. Please enable it in browser settings.';
                    break;
                case 'network':
                    errorMessage = 'Network error. Please check your connection.';
                    break;
                case 'aborted':
                    errorMessage = 'Voice input cancelled.';
                    break;
            }

            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnVoiceError', errorMessage);
            }
        };

        this.recognition.onend = () => {
            this.isRecording = false;
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnVoiceEnd');
            }
        };

        return true;
    },

    // Start listening
    start: function () {
        if (!this.recognition) {
            console.error('Voice recognition not initialized');
            return false;
        }

        if (this.isRecording) {
            return true; // Already recording
        }

        try {
            this.recognition.start();
            return true;
        } catch (e) {
            console.error('Error starting voice recognition:', e);
            return false;
        }
    },

    // Stop listening
    stop: function () {
        if (this.recognition && this.isRecording) {
            try {
                this.recognition.stop();
            } catch (e) {
                console.error('Error stopping voice recognition:', e);
            }
        }
        this.isRecording = false;
    },

    // Toggle recording state
    toggle: function () {
        if (this.isRecording) {
            this.stop();
        } else {
            this.start();
        }
    },

    // Get recording state
    getIsRecording: function () {
        return this.isRecording;
    },

    // Cleanup
    dispose: function () {
        this.stop();
        this.recognition = null;
        this.dotNetRef = null;
    }
};